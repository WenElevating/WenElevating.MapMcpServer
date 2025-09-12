using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WenElevating.MapMcpServer
{
    public class CityCode
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("adcode")]
        public string? Adcode { get; set; }

        [JsonProperty("citycode")]
        public string? Citycode { get; set; }
    }

    /// <summary>
    /// 城市编码小助手
    /// </summary>
    public static class CityCodeHelper
    {
        private static List<CityCode>? CityCodes = [];

        public static async Task<string?> GetCityCodeAsync(string city)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(city);

            if (CityCodes == null || CityCodes?.Count == 0)
            {
                var path = Path.Combine(AppContext.BaseDirectory, "resources", "AMap_adcode_citycode.json");
                var codeStr = await File.ReadAllTextAsync(path);
                CityCodes = JsonConvert.DeserializeObject<List<CityCode>>(codeStr);
            }

            var code = CityCodes?.FirstOrDefault(item =>
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    return false;
                }
                return item.Name.Contains(city);
            });

            return code?.Adcode;
        }
    }
}
