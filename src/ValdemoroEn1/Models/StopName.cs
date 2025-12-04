namespace ValdemoroEn1.Models;

public class StopName(string stopCode, string shortStopCode, string name)
{
    public string StopCode { get; set; } = stopCode;
    public string ShortStopCode { get; set; } = shortStopCode;
    public string Name { get; set; } = name;
}
