using System.Net;
using System.Net.Sockets;

namespace GenICamCameraCapture.Calibration;

/// <summary>
/// 机器人 TCP 通讯链路（客户端 / 服务端双模式）。
/// 协议：简单文本行，UTF-8，CRLF 分隔。
///
/// 发送示例：MOVE_TO 150.00 200.00
/// 接收示例：OK 150.02 199.97
///           ERROR 原因文字
///           ARRIVED 150.02 199.97
/// </summary>
public sealed class TcpRobotLink : IDisposable
{
    public enum LinkMode { Client, Server }

    private TcpClient?            _client;
    private TcpListener?          _listener;
    private NetworkStream?        _stream;
    private StreamWriter?         _writer;
    private StreamReader?         _reader;
    private CancellationTokenSource? _cts;

    public bool IsConnected =>
        _client?.Connected == true && (_stream?.CanWrite ?? false);

    /// <summary>收到一行数据时触发（UI线程外，需 Invoke）。</summary>
    public event EventHandler<string>? DataReceived;

    /// <summary>内部日志（连接状态、错误等），供显示在界面日志框。</summary>
    public event EventHandler<string>? LogOutput;

    // ── 连接 ─────────────────────────────────────────────────

    /// <summary>作为客户端主动连接机器人控制器。</summary>
    public async Task ConnectAsClientAsync(string host, int port, int timeoutMs = 5000)
    {
        Disconnect();
        _client = new TcpClient { NoDelay = true };
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            await _client.ConnectAsync(host, port, cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"连接超时（{timeoutMs} ms）：{host}:{port}");
        }
        InitStreams();
        StartReceiveLoop();
        Emit($"[TCP] 已连接到机器人控制器 {host}:{port}");
    }

    /// <summary>作为服务端等待机器人连接（阻塞至连接建立或超时）。</summary>
    public async Task StartServerAsync(int port, int timeoutMs = 60000)
    {
        Disconnect();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start(1);
        Emit($"[TCP] 等待机器人连接，监听端口 {port}...");
        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            _client = await _listener.AcceptTcpClientAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            _listener.Stop();
            throw new TimeoutException($"等待连接超时（{timeoutMs / 1000} s）");
        }
        _client.NoDelay = true;
        _listener.Stop();
        InitStreams();
        StartReceiveLoop();
        Emit($"[TCP] 机器人已连接：{_client.Client.RemoteEndPoint}");
    }

    // ── 发送 ─────────────────────────────────────────────────

    /// <summary>发送一行命令（自动加 CRLF）。</summary>
    public async Task SendAsync(string command)
    {
        if (_writer == null) throw new InvalidOperationException("TCP 未连接");
        await _writer.WriteLineAsync(command);
        Emit($"[发送] {command}");
    }

    /// <summary>发送命令并等待含 expectedKeyword 的响应行，超时返回 null。</summary>
    public async Task<string?> SendAndWaitAsync(
        string command, string expectedKeyword = "OK", int timeoutMs = 5000)
    {
        var tcs = new TaskCompletionSource<string?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(object? _, string line)
        {
            if (line.Contains(expectedKeyword, StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
                tcs.TrySetResult(line);
        }

        DataReceived += Handler;
        try
        {
            await SendAsync(command);
            using var cts = new CancellationTokenSource(timeoutMs);
            cts.Token.Register(() => tcs.TrySetResult(null));
            return await tcs.Task;
        }
        finally
        {
            DataReceived -= Handler;
        }
    }

    // ── 断开 ─────────────────────────────────────────────────

    public void Disconnect()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _writer?.Dispose();
        _reader?.Dispose();
        _stream?.Dispose();
        _client?.Dispose();
        _listener?.Stop();

        _cts     = null;
        _writer  = null;
        _reader  = null;
        _stream  = null;
        _client  = null;
        _listener = null;

        Emit("[TCP] 已断开");
    }

    public void Dispose() => Disconnect();

    // ── 内部 ─────────────────────────────────────────────────

    private void InitStreams()
    {
        _stream = _client!.GetStream();
        _writer = new StreamWriter(_stream, System.Text.Encoding.UTF8, leaveOpen: true)
                  { AutoFlush = true, NewLine = "\r\n" };
        _reader = new StreamReader(_stream, System.Text.Encoding.UTF8, leaveOpen: true);
    }

    private void StartReceiveLoop()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    string? line = await _reader!.ReadLineAsync(token);
                    if (line == null) break;
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0) continue;
                    Emit($"[接收] {trimmed}");
                    DataReceived?.Invoke(this, trimmed);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Emit($"[TCP] 接收异常: {ex.Message}"); }
        }, token);
    }

    private void Emit(string msg) => LogOutput?.Invoke(this, msg);
}
