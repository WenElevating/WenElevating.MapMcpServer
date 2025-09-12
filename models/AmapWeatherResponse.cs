using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WenElevating.MapMcpServer.models
{
    /// <summary>
    /// 高德天气响应
    /// </summary>
    public class AmapWeatherResponse
    {
        /// <summary>
        /// 返回状态值 -> 为0或1, 1：成功；0：失败
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// 返回结果总数目
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }

        /// <summary>
        /// 返回的状态信息
        /// </summary>
        [JsonProperty("info")]
        public string? Info { get; set; }

        /// <summary>
        /// 返回状态说明,10000代表正确
        /// </summary>
        [JsonProperty("infocode")]
        public string? Infocode { get; set; }

        /// <summary>
        /// 实况天气数据信息
        /// </summary>
        [JsonProperty("lives")]
        public AmapWeatherLives? Lives { get; set; }

        [JsonProperty("forecasts")]
        public AmapWeatherForecasts? Forecasts { get; set; }
    }

    /// <summary>
    /// 实况天气数据信息
    /// </summary>
    public class AmapWeatherLives
    {
        /// <summary>
        /// 省份名
        /// </summary>
        [JsonProperty("province")]
        public string? Province { get; set; }

        /// <summary>
        /// 城市名
        /// </summary>
        [JsonProperty("city")]
        public string? City { get; set; }

        /// <summary>
        /// 区域编码
        /// </summary>
        [JsonProperty("adcode")]
        public string? Adcode { get; set; }

        /// <summary>
        /// 天气现象（汉字描述）
        /// </summary>
        [JsonProperty("weather")]
        public string? Weather { get; set; }

        /// <summary>
        /// 实时气温，单位：摄氏度
        /// </summary>
        [JsonProperty("temperature")]
        public string? Temperature { get; set; }

        /// <summary>
        /// 风向描述
        /// </summary>
        [JsonProperty("winddirection")]
        public string? Winddirection { get; set; }

        /// <summary>
        /// 风力级别，单位：级
        /// </summary>
        [JsonProperty("windpower")]
        public string? Windpower { get; set; }

        /// <summary>
        /// 空气湿度
        /// </summary>
        [JsonProperty("humidity")]
        public string? Humidity { get; set; }

        /// <summary>
        /// 数据发布的时间
        /// </summary>
        [JsonProperty("reporttime")]
        public string? Reporttime { get; set; }
    }

    /// <summary>
    /// 预报天气信息数据
    /// </summary>
    public class AmapWeatherForecasts
    {
        /// <summary>
        /// 省份名称
        /// </summary>
        [JsonProperty("province")]
        public string? Province { get; set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        [JsonProperty("city")]
        public string? City { get; set; }

        /// <summary>
        /// 城市编码
        /// </summary>
        [JsonProperty("adcode")]
        public string? Adcode { get; set; }

        /// <summary>
        /// 预报发布时间
        /// </summary>
        [JsonProperty("reporttime")]
        public string? Reporttime { get; set; }

        /// <summary>
        /// 预报数据 list 结构，元素 cast,按顺序为当天、第二天、第三天、第四天的预报数据
        /// </summary>
        [JsonProperty("casts")]
        public List<AmapWeatherCasts>? Casts { get; set; }
    }

    /// <summary>
    /// 预报数据 list 结构，元素 cast,按顺序为当天、第二天、第三天、第四天的预报数据
    /// </summary>
    public class AmapWeatherCasts
    {
        /// <summary>
        /// 日期
        /// </summary>
        [JsonProperty("date")]
        public string? Date { get; set; }

        /// <summary>
        /// 星期
        /// </summary>
        [JsonProperty("week")]
        public string? Week { get; set; }

        /// <summary>
        /// 白天天气
        /// </summary>
        [JsonProperty("dayweather")]
        public string? DayWeather { get; set; }

        /// <summary>
        /// 夜晚天气
        /// </summary>
        [JsonProperty("nightweather")]
        public string? NightWeather { get; set; }

        /// <summary>
        /// 白天温度
        /// </summary>
        [JsonProperty("daytemp")]
        public string? DayTemp { get; set; }

        /// <summary>
        /// 夜晚温度
        /// </summary>
        [JsonProperty("nighttemp")]
        public string? NightTemp { get; set; }

        /// <summary>
        /// 白天风向
        /// </summary>
        [JsonProperty("daywind")]
        public string? DayWind { get; set; }

        /// <summary>
        /// 夜晚风向
        /// </summary>
        [JsonProperty("nightwind")]
        public string? NightWind { get; set; }

        /// <summary>
        /// 白天风力
        /// </summary>
        [JsonProperty("daypower")]
        public string? DayPower { get; set; }

        /// <summary>
        /// 夜晚风力
        /// </summary>
        [JsonProperty("nightpower")]
        public string? NightPower { get; set; }

        /// <summary>
        /// 白天温度（浮点数）
        ///</summary>
        [JsonProperty("daytemp_float")]
        public string? DayTempFloat { get; set; }

        /// <summary>
        /// 夜晚温度（浮点数）
        ///</summary>
        [JsonProperty("nighttemp_float")]
        public string? NightTempFloat { get; set; }

    }
}
