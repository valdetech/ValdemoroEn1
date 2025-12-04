namespace ValdemoroEn1.Models;

public class StopTimesGroup(string name, string codMode, string line, List<StopTimeName> stopTimeNames)
{
    public string Name { get; set; } = name;
    public string CodMode { get; set; } = codMode;
    public string Line { get; set; } = line;
    public List<StopTimeName> StopTimeNames { get; set; } = stopTimeNames;
}

public class StopTimeName
{
    public string Name { get; set; }
    public List<string> Times { get; set; } = new();
}
