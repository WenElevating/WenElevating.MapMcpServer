using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WenElevating.MapMcpServer
{
    /// <summary>
    /// Poi数据
    /// </summary>
    public struct PoiData
    {
        public int Seq { get; set; }

        public string NewType { get; set; }

        public string BigCategoryCN { get; set; }

        public string MidCategoryCn { get; set; }

        public string SubCategoryCn { get; set; }

        public string BigCategoryEn { get; set; }

        public string MidCategoryEn {  get; set; }

        public string SubCategoryEn { get; set; }
    }

    public static class PoiHelper
    {
        private static List<PoiData>? PoiDatas = [];
        private static readonly string English = "en";
        private static readonly string Chinese = "cn";

        public static async Task<string?> GetPoiTypeAsync(string language, params string[] keyWords)
        {
            if (language != English && language != Chinese)
            {
                throw new NotSupportedException("Only support Chinese or English!");
            }

            if (PoiDatas == null || PoiDatas.Count == 0)
            {
                var path = Path.Combine(AppContext.BaseDirectory, "resources", "AMap_Poi_V1.06_20230208.json");
                var codeStr = await File.ReadAllTextAsync(path);
                PoiDatas = JsonConvert.DeserializeObject<List<PoiData>>(codeStr);
            }

            List<string> poiBuilder = [];
            foreach (var item in keyWords)
            {
                var poiData = PoiDatas?.FirstOrDefault((data) =>
                {
                    if (language == Chinese)
                    {
                        return data.BigCategoryCN.Contains(item) || data.MidCategoryCn.Contains(item) || data.SubCategoryCn.Contains(item);
                    }
                    else
                    {
                        return data.BigCategoryEn.Contains(item) || data.MidCategoryEn.Contains(item) || data.SubCategoryEn.Contains(item);
                    }
                });

                if (poiData != null) 
                {
                    poiBuilder.Add(poiData.Value.NewType);
                }
            }

            return string.Join('|', poiBuilder);
        }
    }
}
