namespace GenICamCameraCapture.Camera;

/// <summary>
/// 统一相机提供者接口 - 华睿/海康/大华 都实现此接口
/// </summary>
public interface ICameraProvider : IDisposable
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// CTI 文件路径
    /// </summary>
    string CtiPath { get; }

    /// <summary>
    /// 初始化（加载 CTI）
    /// </summary>
    void Initialize();

    /// <summary>
    /// 枚举所有可用设备
    /// </summary>
    List<CameraDeviceInfo> EnumerateDevices();

    /// <summary>
    /// 打开指定设备
    /// </summary>
    CameraDevice OpenDevice(string deviceId);

    /// <summary>
    /// 打开第一个可用设备
    /// </summary>
    CameraDevice OpenFirstDevice();
}

/// <summary>
/// 设备基本信息
/// </summary>
public class CameraDeviceInfo
{
    public string DeviceId { get; set; } = "";
    public string InterfaceId { get; set; } = "";
    public string Vendor { get; set; } = "";
    public string Model { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string UserDefinedName { get; set; } = "";

    public override string ToString()
        => $"[{Vendor}] {Model} (SN:{SerialNumber}) - {DisplayName}";
}
