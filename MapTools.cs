using ModelContextProtocol.Server;
using System.ComponentModel;
using WenElevating.MapMcpServer;

namespace MapMcpServer;

/// <summary>
/// 高德地图工具
/// </summary>
[McpServerToolType]
public class MapTools
{
    private static readonly string? apiKey = Environment.GetEnvironmentVariable("GaoDeMapKey", EnvironmentVariableTarget.Machine);
    private static readonly string weatherUrl = "https://restapi.amap.com/v3/weather/weatherInfo";
    private static readonly string geoCodingUrl = "https://restapi.amap.com/v3/geocode/geo";

    [McpServerTool, Description("Get the weather of the city")]
    public static async Task<string> GetWeatherAsync(HttpClient client,
    [Description("City")]string city,
    [Description("Optional values: base/allIf you fill in base: return the current weatherIf you fill in all: return the forecast weather for the next few days")]string extensions,
    [Description("Optional values: JSON, XML")]string output)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city, nameof(city));

        // 初始化响应文本
        string? weatherData;
        try
        {
             // 获取城市编码
            var cityCode = await CityCodeHelper.GetCityCodeAsync(city);
            if (string.IsNullOrEmpty(cityCode))
            {
                return "Not support this city!";
            }

            // 构造参数
            var parameters = InitializeWeatherParameters(cityCode, extensions, output);

            // 构造url
            var url = new Uri($"{weatherUrl}{parameters}");

            // 构造请求
            HttpRequestMessage message = GetMethodRquestMessage(url);

            // 发送请求数据
            HttpResponseMessage responseMessage = await client.SendAsync(message).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            weatherData = await responseMessage.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"请求高德天气数据失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
        Console.WriteLine(weatherData);
        return weatherData;
    }

    [McpServerTool, Description("Provide the ability to convert between structured addresses and their corresponding latitude and longitude.")]
    public static async Task<string> GetGeocoding(HttpClient client,
        [Description("Structured address information, informationThe rules followed are: country, province, city, district/county, town, village, street, house number, housing estate, building, for example: No. 6 Futong East Street, Chaoyang District, Beijing.")]string address,
        [Description("Optional input content includes: the Chinese name of a specified city (e.g., Beijing), the full pinyin of the specified city (beijing), city code (010), ad code (110000), and county-level cities are not supported. When the query content for the specified city is empty, a nationwide address conversion search will be conducted. ...")]string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        try
        {
            // 初始化请求
            var parameters = InitializeGeoCodingParameters(address, city);
            var url = new Uri($"{geoCodingUrl}{parameters}");
            var message = GetMethodRquestMessage(url);

            var responseMessage = await client.SendAsync(message).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            return await responseMessage.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求高德地理数据失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    private static string InitializeGeoCodingParameters(string address, string city)
    {
        string parameters = $"?key={apiKey}";
        if (!string.IsNullOrEmpty(address))
        {
            parameters += $"&address={address}";
        }

        if (!string.IsNullOrEmpty(city))
        {
            parameters += $"&city={city}";
        }
        return parameters;
    }

    private static string InitializeWeatherParameters(string city, string extensions, string output)
    {
        string parameters = $"?city={city}&key={apiKey}";
        if (!string.IsNullOrEmpty(extensions))
        {
            parameters += $"&extensions={extensions}";
        }

        if (!string.IsNullOrEmpty(output))
        {
            parameters += $"&output={output}";
        }
        return parameters;
    }

    /// <summary>
    /// 生成Get请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static HttpRequestMessage GetMethodRquestMessage(Uri url)
    {
        HttpRequestMessage message = new()
        {
            RequestUri = url,
            Method = HttpMethod.Get,
        };
        return message;
    }
}