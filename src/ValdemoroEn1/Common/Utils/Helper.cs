using System.Text;
using System.Text.Json;
using System.Xml;

namespace ValdemoroEn1.Common;

public class Helper
{
    public static T XmlToObject<T>(string xml)
    {
        XmlDocument xmlDoc = new();
        xmlDoc.LoadXml(xml);
        xmlDoc.RemoveChild(xmlDoc.FirstChild);
        var builder = new StringBuilder();
        Newtonsoft.Json.JsonSerializer.Create().Serialize(new CustomJsonWriter(new StringWriter(builder)), xmlDoc);
        string json = builder.ToString();
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }

    //The ideal is to create a database and save the stops.

    public static IEnumerable<StopName> Stops()
    {
        string json = Preferences.Get("stopNames", null);
        if (string.IsNullOrEmpty(json)) return [];

        return JsonSerializer.Deserialize<IEnumerable<StopName>>(json);
    }

    public static void SaveStops(IEnumerable<StopName> stopNames)
    {
        string json = JsonSerializer.Serialize(stopNames);
        Preferences.Set("stopNames", json);
    }

    public static Task OpenMapAsync(string name, double latitude, double longitude)
    {
        var location = new Microsoft.Maui.Devices.Sensors.Location(latitude, longitude);
        var options = new MapLaunchOptions { Name = name };

        try
        {
            return Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            _ = ex.Message;
        }

        return Task.CompletedTask;
    }

    public static async Task<bool> OpenUrlAsync(string url)
    {
        bool result = false;

        try
        {
            result = await Browser.Default.OpenAsync(new Uri(url), BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            _ = ex.Message;
        }

        return result;
    }
}

public class CustomJsonWriter(TextWriter writer) : Newtonsoft.Json.JsonTextWriter(writer)
{
    public override void WritePropertyName(string name)
    {
        if (name.StartsWith('@') || name.StartsWith('#'))
        {
            base.WritePropertyName(name[1..]);
        }
        else
        {
            base.WritePropertyName(name);
        }
    }
}
