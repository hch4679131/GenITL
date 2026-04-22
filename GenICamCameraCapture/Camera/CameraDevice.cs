using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using GenICamCameraCapture.GenTL;

namespace GenICamCameraCapture.Camera;

/// <summary>
/// 相机设备 - 封装 GenTL 设备操作
/// </summary>
public class CameraDevice : IDisposable
{
    private readonly GenTLApi _api;
    private readonly IntPtr _hDevice;
    private readonly IntPtr _hIface;
    private IntPtr _hDataStream;
    private IntPtr _hEvent;
    private IntPtr _hPort;
    private readonly List<IntPtr> _bufferHandles = new();
    private bool _isGrabbing;
    private CancellationTokenSource? _grabCts;
    private Task? _grabTask;

    // 从 XML 解析出的关键寄存器地址
    private ulong _acqStartAddr;
    private int _acqStartValue = 1;
    private ulong _acqStopAddr;
    private int _acqStopValue = 1;
    private bool _xmlParsed;

    // 曝光时间寄存器
    private ulong _exposureTimeAddr;
    private bool _exposureIsFloat; // true=FloatReg(4 bytes IEEE754), false=IntReg
    private int _exposureRegLength = 4;

    // AcquisitionMode 寄存器
    private ulong _acqModeAddr;
    private int _acqModeRegLength = 4;
    // AcquisitionMode 枚举值映射
    private readonly Dictionary<string, int> _acqModeValues = new(StringComparer.OrdinalIgnoreCase);

    public CameraDeviceInfo Info { get; }
    public bool IsGrabbing => _isGrabbing;

    /// <summary>
    /// 收到新帧时触发
    /// </summary>
    public event Action<FrameData>? OnFrameReceived;

    /// <summary>
    /// 日志事件
    /// </summary>
    public event Action<string>? OnLog;

    public CameraDevice(GenTLApi api, IntPtr hIface, IntPtr hDevice, CameraDeviceInfo info)
    {
        _api = api;
        _hIface = hIface;
        _hDevice = hDevice;
        Info = info;
        _hPort = _api.DevGetPort(_hDevice);
    }

    private void Log(string msg)
    {
        Console.WriteLine(msg);
        OnLog?.Invoke(msg);
    }

    public byte[] ReadRegister(ulong address, long size)
        => _api.GCReadPort(_hPort, address, size);

    public void WriteRegister(ulong address, byte[] data)
        => _api.GCWritePort(_hPort, address, data);

    public int ReadRegisterInt32(ulong address)
    {
        byte[] data = ReadRegister(address, 4);
        if (BitConverter.IsLittleEndian) Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    public void WriteRegisterInt32(ulong address, int value)
    {
        byte[] data = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(data);
        WriteRegister(address, data);
    }

    public string GetXmlUrl()
    {
        uint numUrls = _api.GCGetNumPortURLs(_hPort);
        if (numUrls == 0) return "";
        return _api.GCGetPortURLInfo(_hPort, 0, URL_INFO_CMD.URL_INFO_URL);
    }

    /// <summary>
    /// 从设备读取 GenICam XML
    /// </summary>
    public string ReadXmlFromDevice()
    {
        string url = GetXmlUrl();
        Log($"XML URL: {url}");

        // 支持: "Local:filename.zip;addr;len" 和 "local:///filename.zip;addr;len"
        string body = "";
        if (url.StartsWith("local:///", StringComparison.OrdinalIgnoreCase))
            body = url.Substring("local:///".Length);
        else if (url.StartsWith("local:", StringComparison.OrdinalIgnoreCase))
            body = url.Substring("local:".Length);

        if (string.IsNullOrEmpty(body)) return "";

        // 去掉 URL 查询参数，如 "?SchemaVersion=1.0.0"
        int qIdx = body.IndexOf('?');
        if (qIdx >= 0) body = body.Substring(0, qIdx);

        var parts = body.Split(';');
        if (parts.Length < 3) return "";

        ulong addr = Convert.ToUInt64(parts[1], 16);
        long size = Convert.ToInt64(parts[2], 16);
        byte[] xmlData = ReadRegister(addr, size);

        if (xmlData.Length >= 2 && xmlData[0] == 0x50 && xmlData[1] == 0x4B)
        {
            using var ms = new MemoryStream(xmlData);
            using var archive = new ZipArchive(ms);
            var entry = archive.Entries.FirstOrDefault();
            if (entry != null)
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        return System.Text.Encoding.UTF8.GetString(xmlData).TrimEnd('\0');
    }

    /// <summary>
    /// 解析 XML 获取 AcquisitionStart/Stop/ExposureTime/AcquisitionMode 寄存器地址
    /// 支持 pAddress 间接引用和 IntSwissKnife 公式计算
    /// </summary>
    public void ParseXmlRegisters()
    {
        if (_xmlParsed) return;

        string xml = ReadXmlFromDevice();
        if (string.IsNullOrEmpty(xml))
        {
            Log("警告: 无法读取 GenICam XML");
            return;
        }

        Log($"XML 长度: {xml.Length} 字符");

        try
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = doc.Root!.GetDefaultNamespace();

            // ═══ 1. 建立全局节点索引 ═══
            var allNodes = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in doc.Descendants())
            {
                string? n = node.Attribute("Name")?.Value;
                if (n != null) allNodes.TryAdd(n, node);
            }

            // 已解析值缓存（避免重复读设备）
            var valueCache = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);

            // ═══ 2. 辅助方法 ═══

            // 解析地址字符串（支持 0x 前缀十六进制和纯十进制）
            ulong ParseAddr(string s)
            {
                s = s.Trim();
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return ulong.TryParse(s.AsSpan(2), System.Globalization.NumberStyles.HexNumber,
                        null, out ulong v) ? v : 0;
                }
                if (ulong.TryParse(s, out ulong dec)) return dec;
                return ulong.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out ulong hex) ? hex : 0;
            }

            // 简单公式求值: 支持 +, -, *, / 和变量/常数
            ulong EvalFormula(string formula, Dictionary<string, ulong> vars)
            {
                // 补空格使 tokenizer 能拆开紧凑表达式如 "SBRM+0x4"
                formula = formula.Replace("+", " + ").Replace("-", " - ")
                                 .Replace("*", " * ").Replace("/", " / ")
                                 .Replace("(", " ").Replace(")", " ");
                var tokens = formula.Split((char[])[' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                ulong result = 0;
                char op = '+';
                foreach (var tok in tokens)
                {
                    if (tok is "+" or "-" or "*" or "/") { op = tok[0]; continue; }
                    ulong val = vars.TryGetValue(tok, out ulong vv) ? vv : ParseAddr(tok);
                    result = op switch
                    {
                        '+' => result + val,
                        '-' => result - val,
                        '*' => result * val,
                        '/' => val != 0 ? result / val : result,
                        _ => result
                    };
                }
                return result;
            }

            // 获取节点的当前值（可能需要从设备读寄存器，或计算公式）
            ulong GetNodeValue(string nodeName, int depth = 0)
            {
                if (depth > 10) return 0;
                if (valueCache.TryGetValue(nodeName, out ulong cached)) return cached;
                if (!allNodes.TryGetValue(nodeName, out var node)) return 0;

                ulong result = 0;
                string local = node.Name.LocalName;

                if (local is "IntSwissKnife" or "SwissKnife")
                {
                    // 收集变量
                    var vars = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
                    foreach (var pv in node.Elements(ns + "pVariable"))
                    {
                        string? vName = pv.Attribute("Name")?.Value;
                        string? vRef = pv.Value?.Trim();
                        if (vName != null && vRef != null)
                            vars[vName] = GetNodeValue(vRef, depth + 1);
                    }
                    foreach (var ce in node.Elements(ns + "Constant"))
                    {
                        string? vName = ce.Attribute("Name")?.Value;
                        if (vName != null) vars[vName] = ParseAddr(ce.Value);
                    }
                    string? formula = node.Element(ns + "Formula")?.Value?.Trim();
                    if (formula != null)
                        result = EvalFormula(formula, vars);
                }
                else
                {
                    // 直接 <Value>
                    var valElem = node.Element(ns + "Value");
                    if (valElem != null)
                    {
                        result = ParseAddr(valElem.Value);
                    }
                    else
                    {
                        // 有地址 → 从设备读
                        ulong addr = GetNodeAddress(nodeName, depth);
                        if (addr != 0)
                        {
                            try
                            {
                                int len = 4;
                                var le = node.Element(ns + "Length");
                                if (le != null) int.TryParse(le.Value.Trim(), out len);
                                byte[] data = ReadRegister(addr, len);
                                if (data.Length >= 8)
                                {
                                    if (BitConverter.IsLittleEndian) Array.Reverse(data, 0, 8);
                                    result = BitConverter.ToUInt64(data, 0);
                                }
                                else if (data.Length >= 4)
                                {
                                    if (BitConverter.IsLittleEndian) Array.Reverse(data, 0, 4);
                                    result = BitConverter.ToUInt32(data, 0);
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            var pVal = node.Element(ns + "pValue")?.Value?.Trim();
                            if (pVal != null) result = GetNodeValue(pVal, depth + 1);
                        }
                    }
                }

                valueCache[nodeName] = result;
                return result;
            }

            // 获取节点的寄存器地址（跟踪 <Address> / <pAddress> 链）
            ulong GetNodeAddress(string nodeName, int depth = 0)
            {
                if (depth > 10) return 0;
                if (!allNodes.TryGetValue(nodeName, out var node)) return 0;

                var addrElem = node.Element(ns + "Address");
                if (addrElem != null) return ParseAddr(addrElem.Value);

                var pAddr = node.Element(ns + "pAddress")?.Value?.Trim();
                if (pAddr != null) return GetNodeValue(pAddr, depth + 1);

                return 0;
            }

            // ═══ 3. 查找 Command: AcquisitionStart / AcquisitionStop ═══
            foreach (var cmd in doc.Descendants().Where(e => e.Name.LocalName == "Command"))
            {
                string? name = cmd.Attribute("Name")?.Value;
                if (name == null) continue;

                string? pValue = cmd.Element(ns + "pValue")?.Value?.Trim();
                string? cmdValStr = cmd.Element(ns + "CommandValue")?.Value;
                int cmdVal = 1;
                if (cmdValStr != null) int.TryParse(cmdValStr, out cmdVal);

                ulong cmdAddr = 0;
                if (pValue != null)
                    cmdAddr = GetNodeAddress(pValue, 0);

                if (name == "AcquisitionStart" && cmdAddr != 0)
                {
                    _acqStartAddr = cmdAddr;
                    _acqStartValue = cmdVal;
                    Log($"找到 AcquisitionStart: addr=0x{cmdAddr:X}, value={cmdVal}");
                }
                else if (name == "AcquisitionStop" && cmdAddr != 0)
                {
                    _acqStopAddr = cmdAddr;
                    _acqStopValue = cmdVal;
                    Log($"找到 AcquisitionStop: addr=0x{cmdAddr:X}, value={cmdVal}");
                }
            }

            // ═══ 4. 查找 ExposureTime ═══
            foreach (var node in doc.Descendants().Where(e =>
                e.Name.LocalName is "Float" or "FloatReg" or "Integer" or "IntReg"))
            {
                string? name = node.Attribute("Name")?.Value;
                if (name is not ("ExposureTime" or "ExposureTimeAbs" or "ExposureTimeRaw")) continue;

                ulong addr = GetNodeAddress(name, 0);
                if (addr == 0)
                {
                    // pValue 间接引用
                    var pVal = node.Element(ns + "pValue")?.Value?.Trim();
                    if (pVal != null) addr = GetNodeAddress(pVal, 0);
                    if (addr != 0 && allNodes.TryGetValue(pVal!, out var refNode))
                        _exposureIsFloat = refNode.Name.LocalName is "FloatReg" or "Float";
                }
                else
                {
                    _exposureIsFloat = node.Name.LocalName is "Float" or "FloatReg";
                }

                if (addr != 0)
                {
                    _exposureTimeAddr = addr;
                    var le = node.Element(ns + "Length");
                    if (le != null) int.TryParse(le.Value.Trim(), out _exposureRegLength);
                    Log($"找到 {name}: addr=0x{addr:X}, float={_exposureIsFloat}, len={_exposureRegLength}");
                    break;
                }
            }

            // ═══ 5. 查找 AcquisitionMode ═══
            foreach (var enumNode in doc.Descendants().Where(e => e.Name.LocalName == "Enumeration"))
            {
                string? name = enumNode.Attribute("Name")?.Value;
                if (name != "AcquisitionMode") continue;

                var pVal = enumNode.Element(ns + "pValue")?.Value?.Trim();
                if (pVal != null)
                {
                    ulong modeAddr = GetNodeAddress(pVal, 0);
                    if (modeAddr != 0)
                    {
                        _acqModeAddr = modeAddr;
                        if (allNodes.TryGetValue(pVal, out var refNode))
                        {
                            var le = refNode.Element(ns + "Length");
                            if (le != null) int.TryParse(le.Value.Trim(), out _acqModeRegLength);
                        }
                        Log($"找到 AcquisitionMode: addr=0x{modeAddr:X}");
                    }
                }

                foreach (var entry in enumNode.Elements().Where(e => e.Name.LocalName == "EnumEntry"))
                {
                    string? entryName = entry.Attribute("Name")?.Value;
                    var valElem = entry.Element(ns + "Value");
                    if (entryName != null && valElem != null && int.TryParse(valElem.Value.Trim(), out int v))
                        _acqModeValues[entryName] = v;
                }

                if (_acqModeValues.Count > 0)
                    Log($"AcquisitionMode 枚举值: {string.Join(", ", _acqModeValues.Select(kv => $"{kv.Key}={kv.Value}"))}");

                break;
            }

            _xmlParsed = true;
        }
        catch (Exception ex)
        {
            Log($"XML 解析异常: {ex.Message}");
        }
    }

    public void SendAcquisitionStart()
    {
        if (_acqStartAddr != 0)
        {
            WriteRegisterInt32(_acqStartAddr, _acqStartValue);
            Log("已发送 AcquisitionStart");
        }
        else
        {
            Log("警告: AcquisitionStart 地址未知");
        }
    }

    public void SendAcquisitionStop()
    {
        if (_acqStopAddr != 0)
        {
            WriteRegisterInt32(_acqStopAddr, _acqStopValue);
            Log("已发送 AcquisitionStop");
        }
    }

    /// <summary>
    /// 读取当前曝光时间（微秒）
    /// </summary>
    public double GetExposureTime()
    {
        if (_exposureTimeAddr == 0) return -1;
        byte[] data = ReadRegister(_exposureTimeAddr, _exposureRegLength);
        if (_exposureIsFloat && data.Length >= 4)
        {
            if (BitConverter.IsLittleEndian) Array.Reverse(data, 0, 4);
            return BitConverter.ToSingle(data, 0);
        }
        else
        {
            if (BitConverter.IsLittleEndian) Array.Reverse(data, 0, Math.Min(4, data.Length));
            return BitConverter.ToInt32(data, 0);
        }
    }

    /// <summary>
    /// 设置曝光时间（微秒）
    /// </summary>
    public void SetExposureTime(double microseconds)
    {
        if (_exposureTimeAddr == 0)
        {
            Log("警告: ExposureTime 寄存器地址未知");
            return;
        }

        byte[] data;
        if (_exposureIsFloat)
        {
            data = BitConverter.GetBytes((float)microseconds);
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
        }
        else
        {
            data = BitConverter.GetBytes((int)microseconds);
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
        }
        WriteRegister(_exposureTimeAddr, data);
        Log($"曝光时间已设置: {microseconds} μs");
    }

    /// <summary>
    /// 曝光时间寄存器是否已找到
    /// </summary>
    public bool HasExposureControl => _exposureTimeAddr != 0;

    /// <summary>
    /// 设置 AcquisitionMode 为 Continuous（连续自由运行）
    /// </summary>
    public void SetAcquisitionModeContinuous()
    {
        if (_acqModeAddr == 0)
        {
            Log("警告: AcquisitionMode 寄存器地址未知，跳过");
            return;
        }

        if (_acqModeValues.TryGetValue("Continuous", out int val))
        {
            WriteRegisterInt32(_acqModeAddr, val);
            Log($"AcquisitionMode 已设为 Continuous (value={val})");
        }
        else
        {
            // 常见默认值: Continuous=0
            WriteRegisterInt32(_acqModeAddr, 0);
            Log("AcquisitionMode 设为 0 (假定 Continuous)");
        }
    }

    /// <summary>
    /// 开始采集
    /// </summary>
    public void StartGrab(int bufferCount = 10)
    {
        if (_isGrabbing)
            throw new InvalidOperationException("已在采集中");

        // 先解析 XML 获取寄存器地址
        ParseXmlRegisters();

        // 设置连续采集模式（不依赖外部触发）
        SetAcquisitionModeContinuous();

        uint numStreams = _api.DevGetNumDataStreams(_hDevice);
        if (numStreams == 0)
            throw new GenTLException(GC_ERROR.GC_ERR_NO_DATA, "设备没有 DataStream");

        string streamId = _api.DevGetDataStreamID(_hDevice, 0);
        _hDataStream = _api.DevOpenDataStream(_hDevice, streamId);

        long payloadSize;
        try
        {
            payloadSize = _api.DSGetInfoLong(_hDataStream, STREAM_INFO_CMD.STREAM_INFO_PAYLOAD_SIZE);
        }
        catch
        {
            payloadSize = 4096 * 1024;
        }
        if (payloadSize <= 0) payloadSize = 4096 * 1024;

        Log($"Payload Size: {payloadSize}");

        _bufferHandles.Clear();
        for (int i = 0; i < bufferCount; i++)
        {
            IntPtr hBuffer = _api.DSAllocAndAnnounceBuffer(_hDataStream, payloadSize);
            _bufferHandles.Add(hBuffer);
            _api.DSQueueBuffer(_hDataStream, hBuffer);
        }

        _hEvent = _api.GCRegisterEvent(_hDataStream, EVENT_TYPE.EVENT_NEW_BUFFER);

        // 主机端就绪
        _api.DSStartAcquisition(_hDataStream);

        // ★ 命令相机开始发送数据
        SendAcquisitionStart();

        _isGrabbing = true;

        _grabCts = new CancellationTokenSource();
        _grabTask = Task.Factory.StartNew(() => GrabLoop(_grabCts.Token),
            _grabCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        Log("采集已启动");
    }

    /// <summary>
    /// 停止采集
    /// </summary>
    public void StopGrab()
    {
        if (!_isGrabbing) return;
        _isGrabbing = false;

        // ★ 先命令相机停止
        try { SendAcquisitionStop(); } catch { }

        _grabCts?.Cancel();

        if (_hEvent != IntPtr.Zero)
            _api.EventKill(_hEvent);

        _grabTask?.Wait(3000);

        try { _api.DSStopAcquisition(_hDataStream, ACQ_STOP_FLAGS.ACQ_STOP_FLAGS_KILL); } catch { }
        try { _api.DSFlushQueue(_hDataStream, ACQ_QUEUE_TYPE.ACQ_QUEUE_ALL_DISCARD); } catch { }

        if (_hEvent != IntPtr.Zero)
        {
            try { _api.GCUnregisterEvent(_hDataStream, EVENT_TYPE.EVENT_NEW_BUFFER); } catch { }
            _hEvent = IntPtr.Zero;
        }

        foreach (var hBuf in _bufferHandles)
        {
            try { _api.DSRevokeBuffer(_hDataStream, hBuf); } catch { }
        }
        _bufferHandles.Clear();

        try { _api.DSClose(_hDataStream); } catch { }
        _hDataStream = IntPtr.Zero;

        Log("采集已停止");
    }

    private void GrabLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _isGrabbing)
        {
            try
            {
                // ★ 从事件直接拿到已填充的 buffer handle
                bool hasData = _api.EventGetData(_hEvent, out IntPtr hFilledBuffer, 1000);
                if (!hasData || ct.IsCancellationRequested) continue;

                try
                {
                    var bufInfo = _api.DSGetBufferInfo(_hDataStream, hFilledBuffer);
                    if (bufInfo.SizeFilled > 0 && !bufInfo.IsIncomplete)
                    {
                        byte[] imageData = new byte[bufInfo.SizeFilled];
                        Marshal.Copy(bufInfo.Base, imageData, 0, (int)bufInfo.SizeFilled);

                        var frame = new FrameData
                        {
                            Width = bufInfo.Width,
                            Height = bufInfo.Height,
                            Data = imageData,
                            FrameId = bufInfo.FrameId,
                            Timestamp = bufInfo.Timestamp,
                            PixelFormat = bufInfo.PixelFormat,
                        };

                        OnFrameReceived?.Invoke(frame);
                    }
                }
                catch (Exception ex)
                {
                    Log($"处理帧异常: {ex.Message}");
                }

                // 用完重新入队
                try { _api.DSQueueBuffer(_hDataStream, hFilledBuffer); } catch { }
            }
            catch (GenTLException ex) when (ex.ErrorCode == GC_ERROR.GC_ERR_ABORT)
            {
                break;
            }
            catch (GenTLException ex) when (ex.ErrorCode == GC_ERROR.GC_ERR_TIMEOUT)
            {
                continue;
            }
            catch (Exception ex)
            {
                Log($"采集异常: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        StopGrab();
        try { _api.DevClose(_hDevice); } catch { }
    }
}

/// <summary>
/// 帧数据
/// </summary>
public class FrameData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public long FrameId { get; set; }
    public long Timestamp { get; set; }
    public uint PixelFormat { get; set; }

    /// <summary>
    /// 保存为原始文件
    /// </summary>
    public void SaveRaw(string path)
    {
        File.WriteAllBytes(path, Data);
    }

    public override string ToString()
        => $"Frame#{FrameId} {Width}x{Height} Size={Data.Length} PF=0x{PixelFormat:X8}";
}
