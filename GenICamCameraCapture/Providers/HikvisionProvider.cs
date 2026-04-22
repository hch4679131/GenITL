using System.IO;
using GenICamCameraCapture.Camera;
using GenICamCameraCapture.GenTL;

namespace GenICamCameraCapture.Providers;

/// <summary>
/// 海康相机 GenTL 提供者
/// </summary>
public class HikvisionProvider : ICameraProvider
{
    private GenTLApi? _api;
    private IntPtr _hTL;
    private readonly List<(string ifaceId, IntPtr hIface)> _interfaces = new();

    public string ProviderName => "海康 (Hikvision)";
    public string CtiPath { get; }

    public HikvisionProvider(string ctiPath)
    {
        CtiPath = ctiPath;
    }

    public HikvisionProvider()
        : this(Path.Combine(AppContext.BaseDirectory, @"GenTL\Runtime\MVProducerGEV.cti"))
    {
    }

    public void Initialize()
    {
        _api = new GenTLApi();
        _api.Load(CtiPath);
        _api.GCInitLib();
        _hTL = _api.TLOpen();
        Console.WriteLine($"[海康] CTI 已加载: {CtiPath}");
    }

    public List<CameraDeviceInfo> EnumerateDevices()
    {
        if (_api == null) throw new InvalidOperationException("未初始化");

        var devices = new List<CameraDeviceInfo>();

        _api.TLUpdateInterfaceList(_hTL, 3000);
        uint numIfaces = _api.TLGetNumInterfaces(_hTL);
        Console.WriteLine($"[海康] 发现 {numIfaces} 个接口");

        for (uint i = 0; i < numIfaces; i++)
        {
            string ifaceId = _api.TLGetInterfaceID(_hTL, i);
            IntPtr hIface = _api.TLOpenInterface(_hTL, ifaceId);
            _interfaces.Add((ifaceId, hIface));

            _api.IFUpdateDeviceList(hIface, 3000);
            uint numDevices = _api.IFGetNumDevices(hIface);

            for (uint j = 0; j < numDevices; j++)
            {
                string deviceId = _api.IFGetDeviceID(hIface, j);
                var info = new CameraDeviceInfo
                {
                    DeviceId = deviceId,
                    InterfaceId = ifaceId,
                    Vendor = _api.IFGetDeviceInfo(hIface, deviceId, DEVICE_INFO_CMD.DEVICE_INFO_VENDOR),
                    Model = _api.IFGetDeviceInfo(hIface, deviceId, DEVICE_INFO_CMD.DEVICE_INFO_MODEL),
                    SerialNumber = _api.IFGetDeviceInfo(hIface, deviceId, DEVICE_INFO_CMD.DEVICE_INFO_SERIAL_NUMBER),
                    DisplayName = _api.IFGetDeviceInfo(hIface, deviceId, DEVICE_INFO_CMD.DEVICE_INFO_DISPLAYNAME),
                    UserDefinedName = _api.IFGetDeviceInfo(hIface, deviceId, DEVICE_INFO_CMD.DEVICE_INFO_USER_DEFINED_NAME),
                };
                Console.WriteLine($"    设备[{j}]: {info}");
                devices.Add(info);
            }
        }

        return devices;
    }

    public CameraDevice OpenDevice(string deviceId)
    {
        if (_api == null) throw new InvalidOperationException("未初始化");

        foreach (var (ifaceId, hIface) in _interfaces)
        {
            try
            {
                IntPtr hDevice = _api.IFOpenDevice(hIface, deviceId);
                var info = new CameraDeviceInfo
                {
                    DeviceId = deviceId,
                    InterfaceId = ifaceId,
                };
                Console.WriteLine($"[海康] 已打开设备: {deviceId}");
                return new CameraDevice(_api, hIface, hDevice, info);
            }
            catch (GenTLException)
            {
                continue;
            }
        }

        throw new GenTLException(GC_ERROR.GC_ERR_INVALID_ID, $"设备未找到: {deviceId}");
    }

    public CameraDevice OpenFirstDevice()
    {
        var devices = EnumerateDevices();
        if (devices.Count == 0)
            throw new GenTLException(GC_ERROR.GC_ERR_NO_DATA, "没有找到任何设备");
        return OpenDevice(devices[0].DeviceId);
    }

    public void Dispose()
    {
        foreach (var (_, hIface) in _interfaces)
        {
            try { _api?.IFClose(hIface); } catch { }
        }
        _interfaces.Clear();

        if (_hTL != IntPtr.Zero)
        {
            try { _api?.TLClose(_hTL); } catch { }
            _hTL = IntPtr.Zero;
        }

        _api?.Dispose();
        _api = null;
    }
}
