using ModelContextProtocol.Server;
using System.ComponentModel;
using System.IO;
using System.Net;
using WenElevating.MapMcpServer;

namespace MapMcpServer;

/// <summary>
/// 高德地图工具
/// </summary>
[McpServerToolType]
public class MapTools
{
    private static readonly string? apiKey = Environment.GetEnvironmentVariable("GaoDeMapKey", EnvironmentVariableTarget.Machine);
    private static readonly string ipUrl = "https://restapi.amap.com/v3/ip";
    private static readonly string weatherUrl = "https://restapi.amap.com/v3/weather/weatherInfo";
    private static readonly string geoCodingUrl = "https://restapi.amap.com/v3/geocode/geo";
    private static readonly string walkPathPlaningUrl = "https://restapi.amap.com/v3/direction/walking";
    private static readonly string busPathPlaningUrl = "https://restapi.amap.com/v3/direction/transit/integrated";
    private static readonly string carPathPlaningUrl = "https://restapi.amap.com/v3/direction/driving";
    private static readonly string bicyclingPathPlaningUrl = "https://restapi.amap.com/v4/direction/bicycling";
    private static readonly string keyWordSearchUrl = "https://restapi.amap.com/v3/place/text";
    private static readonly string distanceUrl = "https://restapi.amap.com/v3/distance";
    private static readonly string code = "code";
    private static readonly string word = "word";

    /// <summary>
    /// 定位IP的所在位置
    /// </summary>
    /// <param name="client"></param>
    /// <param name="ip"></param>
    /// <returns></returns>
    [McpServerTool, Description("Based on the IP address input by the user, it can quickly help the user locate the location of the IP.")]
    public static async Task<string> GetAddressAsync(HttpClient client, 
        [Description("The IP address to be searched (domestic only).If the user does not fill in the IP address, the request in the customer's http will be taken for location")]string ip = "")
    {
        try
        {
            // 初始化请求
            var parameters = InitializeIPParameters(ip);
            var url = new Uri($"{ipUrl}{parameters}");
            var message = GetMethodRquestMessage(url);
            return await SendMessageAsync(client, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求高德定位数据失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    /// <summary>
    /// 获取指定城市的天气信息
    /// </summary>
    /// <param name="client">http客户端</param>
    /// <param name="city">城市</param>
    /// <param name="extensions">base/all -> 实时天气/天气预报</param>
    /// <param name="output">输出格式 -> JSON/XML</param>
    /// <returns></returns>
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

    /// <summary>
    /// 获取指定位置的地理信息
    /// </summary>
    /// <param name="client">http客户端</param>
    /// <param name="address">结构化地址信息，规则遵循：国家、省份、城市、区县、城镇、乡村、街道、门牌号码、屋邨、大厦，如：北京市朝阳区阜通东大街6号。</param>
    /// <param name="city">指定查询的城市，可选输入内容包括：指定城市的中文（如北京）、指定城市的中文全拼（beijing）、citycode（010）、adcode（110000），不支持县级市。当指定城市查询内容为空时，会进行全国范围内的地址转换检索。</param>
    /// <returns></returns>
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
            return await SendMessageAsync(client, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求高德地理数据失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    /// <summary>
    /// 步行路径规划 API 可以规划100km 以内的步行通勤方案，并且返回通勤方案的数据。最大支持 100km 的步行路线规划。
    /// </summary>
    /// <param name="client">http客户端</param>
    /// <param name="origin">出发点</param>
    /// <param name="destination">目的地</param>
    /// <returns></returns>
    [McpServerTool, Description("The walking path planning API can plan walking commuting solutions within 100 kilometers and return the data of the commuting plan. It supports walking route planning for up to 100 kilometers.")]
    public static async Task<string> GetWalkingPathPlanning(HttpClient client,
        [Description("Rules: lon, lat (longitude, latitude), separated by ',', such as 117.500244, 40.417801. The decimal point for longitude and latitude should not exceed 6 digits.")]string origin, [Description("Rules: lon, lat (longitude, latitude), separated by ',', such as 117.500244, 40.417801. The decimal point for longitude and latitude should not exceed 6 digits.")]string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        try
        {
            var parameters = InitializeWalkPathPlaningParametes(origin, destination);
            var url = new Uri($"{walkPathPlaningUrl}{parameters}");
            HttpRequestMessage message = GetMethodRquestMessage(url);
            return await SendMessageAsync(client, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求步行路径规划失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    /// <summary>
    /// 获取公交路径规划
    /// </summary>
    /// <param name="client"></param>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="city"></param>
    /// <param name="cityd"></param>
    /// <param name="strategy"></param>
    /// <param name="nightflag"></param>
    /// <param name="date"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    [McpServerTool, Description("The public transport route planning API can plan commuting solutions that integrate various types of public transportation (trains, buses, subways) and return the data of the commuting plans.")]
    public static async Task<string> GetBusPathPlaning(HttpClient client,
        [Description("Rules: lon, lat (longitude, latitude), separated by ',', such as 117.500244, 40.417801. The decimal point for longitude and latitude should not exceed 6 digits.")] string origin, [Description("Rules: lon, lat (longitude, latitude), separated by ',', such as 117.500244, 40.417801. The decimal point for longitude and latitude should not exceed 6 digits.")] string destination, [Description("Starting city in urban/inter-city planning, Currently supports the starting cities for urban bus transfers/intercity buses. Optional values: city name/city code")]string city,[Description("Destination city in intercity bus planning, Required parameters for intercity bus planning.Optional values: city name/citycode")]string cityd,
        [Description("Public transport transfer strategy, Optional values:0: Fastest mode;1: Most economical mode;2: Least transfers mode;3: Least walking mode;5: No subway mode.")]string strategy = "0", [Description("Is the night shift car being calculated? Optional values:0: Do not calculate night shifts;1: Calculate night shifts.")]string nightflag = "0", [Description("Departure date, Filter available bus routes based on departure time and date, format example: date=2014-3-19. Please do not include this parameter in the request when there is no need to set an estimated departure time.")]string date = "",[Description("Departure time, Based on the departure time and date, filter the available bus routes. Example format: time=22:34. Please do not include this parameter in your request when there is no need to set an estimated departure time.")]string time = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityd);

        try
        {
            var parameters = InitializeBusPathPlaningParameters(origin, destination, city, cityd, strategy, nightflag, date, time);
            var url = new Uri($"{busPathPlaningUrl}{parameters}");
            var message = GetMethodRquestMessage(url);
            return await SendMessageAsync(client, message);
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"请求公交路径规划失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    /// <summary>
    /// 获取驾车路径规划
    /// </summary>
    /// <param name="client"></param>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="strategy"></param>
    /// <returns></returns>
    [McpServerTool, Description("The driving route planning API can plan schemes for commuting by small cars and sedans, and return the data of the commuting schemes.")]
    public static async Task<string> GetCarPathPlaning(HttpClient client,
        [Description("Starting point.Longitude comes first, followed by latitude, separated by a comma. The decimal places of longitude and latitude must not exceed six digits. The format is x1,y1|x2,y2|x3,y3.Due to the occurrence of positioning drift in practical use, multiple starting points are allowed for calculating the vehicle's heading angle to resolve such issues.A maximum of three coordinate pairs is allowed, and the distance between each pair of coordinates must exceed 2 meters. Although there is no upper limit on the distance between each pair of coordinates, exceeding 4 meters may lead to inaccuracies. Three points are used to determine the validity of distance and angle; if both are valid, the angle calculated from the first and the last point sets the road-grabbing angle, and the last coordinate pair is used for path planning.")]string origin,
        [Description("Destination.Longitude comes first, latitude comes second, and longitude and latitude are separated by a comma. The decimal part of the coordinates should not exceed 6 digits.")]string destination,  [Description("Driving Strategy Selection.Strategies 10 to 20 below will return multiple path planning results. The Gaode Map APP strategy is also included. It is strongly recommended to choose from this strategy.\r\n\r\nStrategies 0 to 9 below will only return one path planning result.\r\n\r\n\r\n\r\nThe following strategy returns multiple path planning results\r\n\r\n10. The returned result will avoid congestion, have a shorter distance, and try to shorten the time as much as possible, which is consistent with the default policy of Autonavi Maps, that is, no selection is made\r\n\r\n11. Return three results including: the shortest time; The shortest distance; Avoid congestion (Due to better algorithms, it is recommended to use 10 instead)\r\n\r\n12. The returned result takes into account the road conditions and plans the route as much as possible to avoid congestion, which is consistent with the \"congestion avoidance\" strategy of Autonavi Maps\r\n\r\n13. The returned result does not take the expressway, which is consistent with the \"no expressway\" strategy of Autonavi Maps\r\n\r\n14. The returned results should plan routes with as low or even free charges as possible, which is consistent with Autonavi Map's \"avoid charges\" strategy\r\n\r\n15. The returned result takes into account the road conditions, plans the route as much as possible to avoid congestion, and does not take the expressway, which is consistent with the \"avoid congestion & do not take the expressway\" strategy of Autonavi Maps\r\n\r\n16. The returned results should try not to take expressways and plan routes with lower or even free tolls as much as possible, which is consistent with the \"Avoid tolls & Do not take expressways\" strategy of Autonavi Maps\r\n\r\n17. The return path planning results will try to avoid congestion as much as possible and plan route results with lower charges or even free of charge, which is consistent with the \"avoid congestion & avoid charges\" strategy of Autonavi Maps\r\n\r\n18. The returned results should try to avoid congestion, plan routes with lower or even free charges, and avoid taking expressways as much as possible. This is consistent with the \"Avoid congestion & avoid charges & Do not take expressways\" strategy of Autonavi Maps\r\n\r\n19. The returned result will prioritize expressways, which is consistent with the \"expressway priority\" strategy of Autonavi Maps\r\n\r\nThe returned result will prioritize expressways and take into account road conditions to avoid congestion, which is consistent with the \"congestion avoidance & Expressway priority\" strategy of Autonavi Maps\r\n\r\n\r\n\r\nThe strategy below only returns one path planning result\r\n\r\n0. Speed first. This route is not necessarily the shortest\r\n\r\nThe route that prioritizes cost, does not take toll sections, and takes the least time\r\n\r\n2. Conventional fastest, comprehensive distance/time consumption planning results\r\n\r\n3. Speed first. Do not take expressways, such as Jingtong Expressway (due to strategy iteration, it is recommended to use 13).\r\n\r\n4. Avoid traffic congestion, but there might be detours, which could take a longer time\r\n\r\n5. Multiple strategies (simultaneously using the three strategies of speed priority, cost priority, and distance priority to calculate the path).\r\n\r\nIt must be noted that even if three strategies are used for route calculation, one to three path planning information will be returned irregularly depending on the road conditions.\r\n\r\n6. Speed comes first. Do not take expressways, but do not rule out taking other toll sections\r\n\r\n7. Prioritize tolls. Do not take expressways and avoid all toll sections\r\n\r\n8. To avoid traffic congestion and tolls, it is possible to take expressways. Considering the road conditions, do not take congested routes, but there may be detours and longer travel times\r\n\r\n9. Avoid traffic congestion and tolls and do not take expressways")]string strategy = "0")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        try
        {
            var parameters = InitializeCarPathPlaningParameters(origin, destination, strategy);
            var url = new Uri($"{carPathPlaningUrl}{parameters}");
            var message = GetMethodRquestMessage(url);
            return await SendMessageAsync(client, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求驾车路径规划失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    /// <summary>
    /// 骑行路径规划
    /// </summary>
    /// <param name="client"></param>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    [McpServerTool, Description("Cycling route planning is used to plan cycling commuting schemes. When planning, situations such as overpasses, one-way streets, and road closures will be taken into consideration. Supports maximum cycling route planning of 500 kilometers.")]
    public static async Task<string> GetBicyclingPathPlaning(HttpClient client,
        [Description("Rules: lon, lat (longitude, latitude), separated by ',', such as 117.500244, 40.417801. The decimal point for longitude and latitude should not exceed 6 digits.")] string origin, [Description("Rules: lon, lat (longitude, latitude), separated by ',', such as 117.500244, 40.417801. The decimal point for longitude and latitude should not exceed 6 digits.")] string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        try
        {
            var parameters = InitializeBicyclingPathPlaningParameters(origin, destination);
            var url = new Uri($"{bicyclingPathPlaningUrl}{parameters}");
            var message = GetMethodRquestMessage(url);
            return await SendMessageAsync(client, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求骑行路径规划失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    /// <summary>
    /// 可以测量两个经纬度坐标之间的距离,支持驾车、步行以及球面距离测量
    /// </summary>
    /// <param name="client"></param>
    /// <param name="origins"></param>
    /// <param name="destination"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [McpServerTool, Description("The distance measurement API can measure the distance between two longitude and latitude coordinates, supporting driving, walking and spherical distance measurement")]
    public static async Task<string> GetDistanceAsync(HttpClient client,
        [Description("Starting point. Supports 100 coordinate pairs, with coordinate pairs separated by \"/\". Longitude and latitude are separated by \",\"")]string origins,
        [Description("Rules: lon, lat (longitude, latitude), separated by ',', such as 117.500244, 40.417801. The decimal point for longitude and latitude should not exceed 6 digits.")] string destination, [Description("The ways and methods of path calculation.0:\r\nStraight-line distance\r\n\r\n1: Driving navigation distance (only supports domestic coordinates).\r\n\r\nIt must be pointed out that when it is 1, road conditions will be taken into account, so the requested return results may vary at different times.\r\n\r\nThis strategy is basically consistent with the strategy=4 strategy of the driving route planning interface. The strategy is \"Avoid congested routes, but there may be detours, which may take a longer time.\"\r\n\r\nIf you need to achieve the effect of the Autonavi Map client, you can consider using the driving route planning interface\r\n\r\n3: Walking planning distance (Only supports distances within 5 kilometers)")] string type = "1")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origins);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        try
        {
            var parameters = InitializeDistanceParameters(origins, destination, type);
            var url = new Uri($"{distanceUrl}{parameters}");
            var message = GetMethodRquestMessage(url);
            return await SendMessageAsync(client, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求距离测量失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    /// <summary>
    /// 关键字搜索
    /// </summary>
    /// <param name="client"></param>
    /// <param name="keyword"></param>
    /// <param name="poiType"></param>
    /// <param name="poiTypeLanguage"></param>
    /// <returns></returns>
    [McpServerTool, Description("Keyword search: Conduct conditional searches using the keywords of POI, such as KFC, Chaoyang Park, etc. It also supports setting POI type searches, for example: banks")]
    public static async Task<String> GetKeywordSearch(HttpClient client, [Description("Query keywords.Rule: Only one keyword is supported\r\n\r\nIf no city is specified and the search is for generic terms (such as \"food\"), the returned content will be a list of cities and how many results within this city meet the requirements.")]string keyword,
        [Description("Query the POI type. Optional values: Classification code or Chinese characters (If Chinese characters are used, please fill in strictly according to the Chinese characters in the attachment)\r\n\r\nRule: Separate multiple keywords with \"/\"\r\n\r\n\r\nThe classification code consists of six digits and is divided into three parts. The first two digits represent the major category. The two middle numbers represent the middle class. The last two numbers represent the subclasses.\r\n\r\nIf a major category is specified, both the medium and minor categories to which it belongs will be displayed.\r\n\r\nFor example: 010000 is for automotive services (major category)\r\n\r\n010100 is a gas station (medium category)\r\n\r\n010101 is Sinopec (Subcategory)\r\n\r\n010900 is for car rental (medium category)\r\n\r\n\"010901 for car rental return (Subcategory)\r\n\r\nWhen 010000 is specified, medium classes such as 010100 and minor classes such as 010101 will be included. When 010900 is specified, minor classes such as 010901 will be included.")]string poiType = "", [Description("The POI type language for the query only supports Chinese or English, need input cn or en")]string poiTypeLanguage = "cn")
    {
        try
        {
            var parameters = await InitializeKeywordSearchParametersAsync(keyword, poiType, poiTypeLanguage);
            var url = new Uri($"{keyWordSearchUrl}{parameters}");
            var message = GetMethodRquestMessage(url);
            return await SendMessageAsync(client, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"请求关键字搜索失败! 原因：{ex.Message}, 堆栈如下: {ex.StackTrace}");
            return ex.Message;
        }
    }

    /// <summary>
    /// 发送Http请求，并返回响应
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private static async Task<string> SendMessageAsync(HttpClient client, HttpRequestMessage message)
    {
        HttpResponseMessage responseMessage = await client.SendAsync(message).ConfigureAwait(false);
        responseMessage.EnsureSuccessStatusCode();
        return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 设置参数
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string SetParameter(string name, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        return $"&{name}={value}";
    }

    private static async Task<string> InitializeKeywordSearchParametersAsync(string keyword, string types, string poiTypeLanguage)
    {
        string? pois = await PoiHelper.GetPoiTypeAsync(poiTypeLanguage, types);
        string parameters = $"?key={apiKey}";
        parameters += SetParameter(nameof(keyword), keyword);
        parameters += SetParameter(nameof(types), pois);
        parameters += "&city&children=1&offset=20&page=1&extensions=base";
        return parameters;
    }

    /// <summary>
    /// 初始化ip定位参数
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    private static string InitializeIPParameters(string ip)
    {
        string parameters = $"?key={apiKey}";
        parameters += SetParameter(nameof(ip), ip);
        return parameters;
    }

    private static string InitializeDistanceParameters(string origins, string destination, string type)
    {
        string parameters = $"?key={apiKey}";
        parameters += SetParameter(nameof(origins), origins);
        parameters += SetParameter(nameof(destination), destination);
        parameters += SetParameter(nameof(type), type);
        return parameters;
    }

    /// <summary>
    /// 初始化骑行路径规划参数
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    private static string InitializeBicyclingPathPlaningParameters(string origin, string destination)
    {
        string parameters = $"?key={apiKey}";
        parameters += SetParameter(nameof(origin), origin);
        parameters += SetParameter(nameof(destination), destination);
        return parameters;
    }

    /// <summary>
    /// 初始化驾车路径规划参数
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="strategy"></param>
    /// <returns></returns>
    private static string InitializeCarPathPlaningParameters(string origin, string destination, string strategy)
    {
        string parameters = $"?key={apiKey}";
        parameters += SetParameter(nameof(origin), origin);
        parameters += SetParameter(nameof(destination), destination);
        parameters += SetParameter(nameof(strategy), strategy);
        return parameters;
    }


    /// <summary>
    /// 初始化公交路径规划参数
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="city"></param>
    /// <param name="cityd"></param>
    /// <param name="strategy"></param>
    /// <param name="nightflag"></param>
    /// <param name="date"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private static string InitializeBusPathPlaningParameters(string origin, string destination, string city, string cityd, string strategy, string nightflag, string date, string time)
    {
        string parameters = $"?key={apiKey}";
        parameters += SetParameter(nameof(origin), origin);
        parameters += SetParameter(nameof(destination), destination);
        parameters += SetParameter(nameof(city), city);
        parameters += SetParameter(nameof(cityd), cityd);
        parameters += SetParameter(nameof(strategy), strategy);
        parameters += SetParameter(nameof(nightflag), nightflag);
        parameters += SetParameter(nameof(date), date);
        parameters += SetParameter(nameof(time), time);
        return parameters;
    }

    /// <summary>
    /// 初始化步行路径规划参数
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    private static string InitializeWalkPathPlaningParametes(string origin, string destination)
    {
        string parameters = $"?key={apiKey}";
        parameters += $"&origin={origin}";
        parameters += $"&destination={destination}";
        return parameters;
    }

    /// <summary>
    /// 初始化地理编码参数
    /// </summary>
    /// <param name="address"></param>
    /// <param name="city"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 初始化天气参数
    /// </summary>
    /// <param name="city"></param>
    /// <param name="extensions"></param>
    /// <param name="output"></param>
    /// <returns></returns>
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