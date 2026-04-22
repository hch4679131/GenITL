using System.IO;
using GenICamCameraCapture.Camera;

namespace GenICamCameraCapture.Providers;

/// <summary>
/// 大华相机 GenTL 提供者（后续实现，结构已预留）
/// </summary>
public class DahuaProvider : ICameraProvider
{
    public string ProviderName => "大华 (Dahua)";
    public string CtiPath { get; }

    public DahuaProvider(string? ctiPath = null)
    {
        CtiPath = ctiPath ?? Path.Combine(AppContext.BaseDirectory, @"GenTL\Runtime\MVProducerGEV.cti");
    }

    public void Initialize()
    {
        // TODO: 与 HuarayProvider 实现方式相同，加载对应 .cti
        throw new NotImplementedException("大华 Provider 待实现");
    }

    public List<CameraDeviceInfo> EnumerateDevices() => throw new NotImplementedException();
    public CameraDevice OpenDevice(string deviceId) => throw new NotImplementedException();
    public CameraDevice OpenFirstDevice() => throw new NotImplementedException();
    public void Dispose() { }
}
