using System.ComponentModel;

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace HelloWorldCopilot.Plugins;

public class CommonPlugins
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    public CommonPlugins(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    [KernelFunction("GetWeather"), Description("Get the weather for a city.")]
    [return: Description("The weather in the city in JSON format.")]
    public async Task<string> GetWeather([Description("City name to fetch the weather")] string cityName)
    {
        var APIkey = _configuration["OpenWeatherMap:APIKey"];
        var weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={APIkey}";
        using var httpClient = new HttpClient();
        var weather = await httpClient.GetStringAsync(weatherUrl);
        return weather;
    }

    [KernelFunction("GetCurrentLocation"), Description("Get the current location.")]
    [return: Description("The current location.")]
    public async Task<string> GetCurrentLocation()
    {
        var locationUrl = "http://ip-api.com/json";
        var location = await _httpClient.GetStringAsync(locationUrl);
        return location;
    }

    [KernelFunction("GetTime"), Description("Get the current time.")]
    [return: Description("The current time.")]
    public string GetTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }
}
