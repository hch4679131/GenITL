using System.IO;
using GenICamCameraCapture.Camera;

namespace GenICamCameraCapture.Acquisition;

/// <summary>
/// 线扫采集辅助类
/// </summary>
public class LineScanCapture : IDisposable
{
    private CameraDevice? _device;
    private int _frameCount;
    private readonly string _saveDir;

    public int TotalFrames => _frameCount;

    /// <summary>
    /// 收到帧的事件
    /// </summary>
    public event Action<FrameData, int>? OnFrame;

    public LineScanCapture(string saveDirectory = "captures")
    {
        _saveDir = saveDirectory;
        if (!Directory.Exists(_saveDir))
            Directory.CreateDirectory(_saveDir);
    }

    /// <summary>
    /// 绑定相机设备
    /// </summary>
    public void Attach(CameraDevice device)
    {
        _device = device;
        _device.OnFrameReceived += OnFrameReceived;
    }

    /// <summary>
    /// 开始线扫采集
    /// </summary>
    public void Start(int bufferCount = 16)
    {
        if (_device == null) throw new InvalidOperationException("未绑定设备");
        _frameCount = 0;
        _device.StartGrab(bufferCount);
    }

    /// <summary>
    /// 停止采集
    /// </summary>
    public void Stop()
    {
        _device?.StopGrab();
    }

    private void OnFrameReceived(FrameData frame)
    {
        int count = Interlocked.Increment(ref _frameCount);
        Console.WriteLine($"  帧[{count}]: {frame}");
        OnFrame?.Invoke(frame, count);
    }

    /// <summary>
    /// 保存帧数据
    /// </summary>
    public void SaveFrame(FrameData frame, string? filename = null)
    {
        filename ??= $"frame_{frame.FrameId}_{frame.Width}x{frame.Height}.raw";
        string path = Path.Combine(_saveDir, filename);
        frame.SaveRaw(path);
        Console.WriteLine($"  已保存: {path}");
    }

    public void Dispose()
    {
        Stop();
        if (_device != null)
            _device.OnFrameReceived -= OnFrameReceived;
    }
}
