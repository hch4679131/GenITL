namespace GenICamCameraCapture.Models;

public class SolderJoint
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public bool IsSelected { get; set; } = true;
    public string Label => $"J{Id}";
}
