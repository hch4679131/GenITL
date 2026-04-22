using System.Runtime.InteropServices;

namespace GenICamCameraCapture.Acquisition;

/// <summary>
/// 图像缓冲区管理
/// </summary>
public class ImageBuffer : IDisposable
{
    private IntPtr _buffer;

    public int Width { get; }
    public int Height { get; }
    public int Stride { get; }
    public int BytesPerPixel { get; }
    public long Size => (long)Stride * Height;
    public IntPtr Data => _buffer;

    public ImageBuffer(int width, int height, int bytesPerPixel = 1)
    {
        Width = width;
        Height = height;
        BytesPerPixel = bytesPerPixel;
        Stride = width * bytesPerPixel;
        _buffer = Marshal.AllocHGlobal((int)Size);
    }

    /// <summary>
    /// 从帧数据拷贝
    /// </summary>
    public void CopyFrom(byte[] data)
    {
        int copySize = Math.Min(data.Length, (int)Size);
        Marshal.Copy(data, 0, _buffer, copySize);
    }

    /// <summary>
    /// 导出为 byte 数组
    /// </summary>
    public byte[] ToArray()
    {
        byte[] data = new byte[Size];
        Marshal.Copy(_buffer, data, 0, (int)Size);
        return data;
    }

    public void Dispose()
    {
        if (_buffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_buffer);
            _buffer = IntPtr.Zero;
        }
    }
}
