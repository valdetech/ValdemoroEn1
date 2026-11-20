using System.Globalization;
using System.Net.Http.Json;
using Plugin.Firebase.Firestore;
using ValdemoroEn1.Common.Utils;

namespace ValdemoroEn1.Services;

public class ApiService
{
    private const string ApiKeysCollectionPath = "apikeys";
    private const string IQAirFieldName = "IQAir";

    private readonly HttpClient cinemahttpClient = new();
    private readonly HttpClient fuelhttpClient = new();
    private readonly HttpClient iqairhttpClient = new();
    private readonly HttpClient crtmHttpClient = new();
    private readonly HttpClient renfeHttpClient = new();

    public ApiService()
    {
        cinemahttpClient.BaseAddress = new Uri(AppSettings.CinemaUrl);
        cinemahttpClient.Timeout = TimeSpan.FromSeconds(40);

        fuelhttpClient.BaseAddress = new Uri(AppSettings.FuelApiUrl);
        fuelhttpClient.Timeout = TimeSpan.FromSeconds(40);

        iqairhttpClient.BaseAddress = new Uri(AppSettings.IQAirApiUrl);
        iqairhttpClient.Timeout = TimeSpan.FromSeconds(40);

        crtmHttpClient.BaseAddress = new Uri(AppSettings.CrtmApiUrl);
        crtmHttpClient.Timeout = TimeSpan.FromSeconds(40);

        renfeHttpClient.BaseAddress = new Uri(AppSettings.RenfeApiUrl);
        renfeHttpClient.Timeout = TimeSpan.FromSeconds(40);
        renfeHttpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        renfeHttpClient.DefaultRequestHeaders.Add("X-IBM-Client-Id", AppSettings.RenfeApiClientId);
    }

    public async Task<MovieResponse> MovieTimesAsync()
    {
        string movies = await cinemahttpClient.GetStringAsync("cartelera.asp");
        var movieResponse = Helper.XmlToObject<MovieResponse>(movies);
        return movieResponse;
    }

    public Task<GasStationResponse> GasStationsAsync(string idProducto)
    {
        string municipaly = AppSettings.FuelApiMunicipaly;
        return fuelhttpClient.GetFromJsonAsync<GasStationResponse>($"EstacionesTerrestres/FiltroMunicipioProducto/{municipaly}/{idProducto}");
    }

    public Task<List<FuelResponse>> FuelsAsync()
    {
        return fuelhttpClient.GetFromJsonAsync<List<FuelResponse>>("Listados/ProductosPetroliferos");
    }

    public async Task<WeatherResponse> WeatherAsync()
    {
        string key = await GetIQAirApiKeyAsync();
        string country = AppSettings.IQAirApiCountry;
        string state = AppSettings.IQAirApiState;
        string city = AppSettings.IQAirApiCity;

        return await iqairhttpClient.GetFromJsonAsync<WeatherResponse>($"city?city={city}&state={state}" +
            $"&country={country}&key={Uri.EscapeDataString(key)}");
    }

    private static async Task<string> GetIQAirApiKeyAsync()
    {
        var snapshot = await CrossFirebaseFirestore.Current
            .GetCollection(ApiKeysCollectionPath)
            .LimitedTo(1)
            .GetDocumentsAsync<Dictionary<string, object>>();
        var data = snapshot.Documents.FirstOrDefault()?.Data;

        if (data is null)
            throw new InvalidOperationException($"The Firestore collection '{ApiKeysCollectionPath}' is empty.");

        if (!data.TryGetValue(IQAirFieldName, out object value) ||
            value is not string apiKey ||
            string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException($"The Firestore field '{IQAirFieldName}' is missing or empty.");
        }

        return apiKey;
    }

    public async Task<StopTimesResponse> SchedulesAsync(string stopCode)
    {
        StopTimesResponse stopTimes = await crtmHttpClient.GetFromJsonAsync<StopTimesResponse>(
            $"GetStopsTimes.php?codStop={stopCode}&type=1&orderBy=2&stopTimesByIti=3");

        if (stopTimes?.StopTimes is null)
            throw new InvalidDataException("The CRTM stop-times response is missing stop information.");

        bool hasCrtmTimes = stopTimes.StopTimes.Times?.Time is { Count: > 0 };
        if (stopCode != AppSettings.CrtmRenfeStopCode || hasCrtmTimes)
            return stopTimes;

        RenfeStopTimesResponse renfeStopTimes = await renfeHttpClient.GetFromJsonAsync<RenfeStopTimesResponse>(
            $"hcr-cercanias-middleware/estaciones/{AppSettings.RenfeApiCodStation}");

        if (renfeStopTimes?.Station?.Departures is null)
            throw new InvalidDataException("The Renfe stop-times response is missing departures.");

        stopTimes.StopTimes.Times ??= new Times();
        stopTimes.StopTimes.Times.Time = renfeStopTimes.Station.Departures
            .Select(ToTimeStop)
            .ToList();

        return stopTimes;
    }

    private static TimeStop ToTimeStop(RenfeDeparture departure)
    {
        string departureTime = !string.IsNullOrWhiteSpace(departure.RealDepartureTime)
            ? departure.RealDepartureTime
            : !string.IsNullOrWhiteSpace(departure.DepartureTime)
                ? departure.DepartureTime
                : departure.PlannedDepartureTime;

        if (!DateTime.TryParseExact(
                departureTime,
                "dd-MM-yyyy HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedTime))
        {
            throw new InvalidDataException($"The Renfe departure time '{departureTime}' is invalid.");
        }

        return new TimeStop
        {
            Line = new Line
            {
                CodLine = $"5__{departure.Line}___",
                ShortDescription = departure.Line,
                Description = $"{departure.Line}-Cercanías Madrid",
                CodMode = "5"
            },
            Destination = departure.Destination,
            StopTime = parsedTime
        };
    }

    public Task<StopsResponse> StopsAsync()
    {
        string codMunicipality = AppSettings.CrtmApiCodMunicipaly;
        return crtmHttpClient.GetFromJsonAsync<StopsResponse>($"GetStops.php?codMunicipality={codMunicipality}");
    }
}

