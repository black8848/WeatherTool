using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class WeatherResponse
{
    public string? status { get; set; }
    public string? count { get; set; }
    public string? info { get; set; }
    public string? infocode { get; set; }
    public WeatherInfo[]? lives { get; set; }
}

class WeatherInfo
{
    public string? province { get; set; }
    public string? city { get; set; }
    public string? adcode { get; set; }
    public string? weather { get; set; }
    public string? temperature { get; set; }
    public string? winddirection { get; set; }
    public string? windpower { get; set; }
    public string? humidity { get; set; }
    public string? reporttime { get; set; }
    public string? temperature_float { get; set; }
    public string? humidity_float { get; set; }

}

class Learn
{
    static async Task Main(string[] args)
    {

        while (true)
        {

            string weatherJson = await GetWeatherAsync("210112");

            WeatherResponse? weatherData = JsonSerializer.Deserialize<WeatherResponse>(weatherJson);

            if (weatherData != null && weatherData.lives?.Length > 0)
            {
                var weather = weatherData.lives[0];
                Console.WriteLine($"当前时间: {DateTime.Now}");
                Console.WriteLine($"城市: {weather.city}");
                Console.WriteLine($"天气: {weather.weather}");
                Console.WriteLine($"温度: {weather.temperature}°C");
                Console.WriteLine($"湿度: {weather.humidity}%");
                Console.WriteLine($"风向: {weather.winddirection}");
                Console.WriteLine($"风力: {weather.windpower}级\n");
            }

            await Task.Delay(10000);

        }

    }

    static async Task<string> GetWeatherAsync(string city)
    {
        using (var client = new HttpClient())
        {
            try
            {
                const string BASE_URL = "https://restapi.amap.com";
                string? API_KEY = Environment.GetEnvironmentVariable("AMAP_API_KEY");
                string url = $"{BASE_URL}/v3/weather/weatherInfo?key={API_KEY}&city={city}";

                HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (HttpRequestException e)
            {
                return $"请求错误: {e.Message}";
            }
        }

    }
}
