using System.Text.Json.Serialization;

namespace ValdemoroEn1.Services.API.DTO;

public class RenfeStopTimesResponse
{
    [JsonPropertyName("estacion")]
    public RenfeStation Station { get; set; }
}

public class RenfeStation
{
    [JsonPropertyName("salidas")]
    public List<RenfeDeparture> Departures { get; set; }
}

public class RenfeDeparture
{
    [JsonPropertyName("destinoNombre")]
    public string Destination { get; set; }

    [JsonPropertyName("horaSalidaReal")]
    public string RealDepartureTime { get; set; }

    [JsonPropertyName("horaSalidaPlanificada")]
    public string PlannedDepartureTime { get; set; }

    [JsonPropertyName("horaSalida")]
    public string DepartureTime { get; set; }

    [JsonPropertyName("linea")]
    public string Line { get; set; }
}
