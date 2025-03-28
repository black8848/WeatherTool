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

    private static readonly HttpClient client = new HttpClient(); //创建一个 HttpClient() 以供复用，防止重复生成导致端口被挤占等资源浪费
    private static string LiaoNing_ShenYang_HunNan = "210112"; // 浑南区编码(其他城市详见https://lbs.amap.com/api/webservice/download)
    private static string LiaoNing_ShenYang_LiaoZhong = "210115";// 辽中区编码
    private static string LiaoNing_ShenYang_TieXi = "210106"; //铁西区编码

    private const string BASE_URL = "https://restapi.amap.com"; // 高德 API 平台
    private static string? API_KEY = Environment.GetEnvironmentVariable("AMAP_API_KEY"); // API 环境变量设置
    private static int Time = 10000;  //播报间隔时间

    static readonly object consoleLock = new object();
    static async Task Main(string[] args)
    {
        List<Task> tasks = new List<Task>
            {
                DisPlayWeather(LiaoNing_ShenYang_HunNan),
            };
        tasks.Add(DisPlayWeather(LiaoNing_ShenYang_LiaoZhong));
        tasks.Add(DisPlayWeather(LiaoNing_ShenYang_TieXi));
        await Task.WhenAll(tasks);


    }

    static async Task DisPlayWeather(string city)
    {

        while (true)
        {

            var startTime = DateTime.UtcNow;

            string? weatherJson = await GetWeatherAsync(city);

            if (weatherJson == null)
            {
                lock (consoleLock)
                {
                    Console.WriteLine("无法获取天气数据，稍后重试...");
                }
                await Task.Delay(Time);
                continue;
            }

            try
            {
                WeatherResponse? weatherData = JsonSerializer.Deserialize<WeatherResponse>(weatherJson);
                if (weatherData != null && weatherData.lives?.Length > 0)
                {
                    var weather = weatherData.lives[0];
                    lock (consoleLock)
                    {
                        Console.WriteLine($"当前时间: {DateTime.Now}");
                        Console.WriteLine($"城市: {weather.city}");
                        Console.WriteLine($"天气: {weather.weather}");
                        Console.WriteLine($"温度: {weather.temperature}°C");
                        Console.WriteLine($"湿度: {weather.humidity}%");
                        Console.WriteLine($"风向: {weather.winddirection}");
                        Console.WriteLine($"风力: {weather.windpower}级\n");
                    }
                }
            }
            catch (JsonException ex)
            {
                lock (consoleLock)
                {
                    Console.WriteLine($"JSON 解析错误: {ex.Message}");
                }
            }

            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                lock (consoleLock)
                {
                    Console.WriteLine("程序退出...");
                }
                break;
            }

            var elapsedTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            var delayTime = Math.Max(0, 10000 - (int)elapsedTime);  // 确保不会负数

            await Task.Delay(delayTime);

        }
    }

    static async Task<string?> GetWeatherAsync(string city)
    {
        try
        {
            if (string.IsNullOrEmpty(API_KEY)) //增加环境变量检查，防止 API_KEY 获取失败或者为空
            {
                lock (consoleLock)
                {
                    Console.WriteLine("错误: 未设置 AMAP_API_KEY 环境变量");
                }
                return null;
            }

            string url = $"{BASE_URL}/v3/weather/weatherInfo?key={API_KEY}&city={city}";

            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
        catch (HttpRequestException e)
        {
            lock (consoleLock)
            {
                Console.WriteLine($"请求错误: {e.Message}");
            }
            return null;
        }
    }
}
