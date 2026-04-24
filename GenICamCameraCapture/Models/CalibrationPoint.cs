namespace GenICamCameraCapture.Models;

/// <summary>一个9点标定样本：机器人TCP坐标 ↔ 相机像素坐标。</summary>
public class CalibrationPoint
{
    public int   Index    { get; set; }   // 1-9
    public float RobotX   { get; set; }   // mm（机器人世界坐标 X）
    public float RobotY   { get; set; }   // mm（机器人世界坐标 Y）
    public float PixelX   { get; set; }   // 像素坐标 X
    public float PixelY   { get; set; }   // 像素坐标 Y
    public bool  IsMarked { get; set; }   // 像素坐标是否已标记
}
