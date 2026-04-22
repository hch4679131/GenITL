namespace GenICamCameraCapture.Camera;

/// <summary>
/// 相机管理器 - 管理多个厂商 Provider
/// </summary>
public class CameraManager : IDisposable
{
    private readonly List<ICameraProvider> _providers = new();

    /// <summary>
    /// 注册一个相机提供者
    /// </summary>
    public void RegisterProvider(ICameraProvider provider)
    {
        _providers.Add(provider);
    }

    /// <summary>
    /// 初始化所有提供者
    /// </summary>
    public void InitializeAll()
    {
        foreach (var p in _providers)
        {
            try
            {
                p.Initialize();
                Console.WriteLine($"[{p.ProviderName}] 初始化成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{p.ProviderName}] 初始化失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 枚举所有设备
    /// </summary>
    public List<(ICameraProvider Provider, CameraDeviceInfo Info)> EnumerateAllDevices()
    {
        var result = new List<(ICameraProvider, CameraDeviceInfo)>();
        foreach (var p in _providers)
        {
            try
            {
                var devices = p.EnumerateDevices();
                foreach (var d in devices)
                    result.Add((p, d));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{p.ProviderName}] 枚举失败: {ex.Message}");
            }
        }
        return result;
    }

    /// <summary>
    /// 根据设备信息打开相机
    /// </summary>
    public CameraDevice OpenDevice(ICameraProvider provider, CameraDeviceInfo info)
    {
        return provider.OpenDevice(info.DeviceId);
    }

    public void Dispose()
    {
        foreach (var p in _providers)
            p.Dispose();
        _providers.Clear();
    }
}
