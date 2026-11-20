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

    public Task<StopTimesResponse> SchedulesAsync(string stopCode)
    {
        return crtmHttpClient.GetFromJsonAsync<StopTimesResponse>($"GetStopsTimes.php?codStop={stopCode}&type=1&orderBy=2&stopTimesByIti=3");
    }

    public Task<StopsResponse> StopsAsync()
    {
        string codMunicipality = AppSettings.CrtmApiCodMunicipaly;
        return crtmHttpClient.GetFromJsonAsync<StopsResponse>($"GetStops.php?codMunicipality={codMunicipality}");
    }
}

