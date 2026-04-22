using System.Runtime.InteropServices;

namespace GenICamCameraCapture.GenTL;

/// <summary>
/// 缓冲区信息（用于自定义封装）
/// </summary>
public class BufferInfo
{
    public IntPtr Base { get; set; }
    public long Size { get; set; }
    public long SizeFilled { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FrameId { get; set; }
    public long Timestamp { get; set; }
    public bool IsIncomplete { get; set; }
    public uint PixelFormat { get; set; }
}
