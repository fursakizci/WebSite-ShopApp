using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopApp.WebUI.Extensions
{
    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonConvert.SerializeObject(value);
        }
        public static T Get<T>(this ITempDataDictionary tempDate, string key) where T : class
        {
            object o;
            tempDate.TryGetValue(key, out o);
            return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
        }
    }
}
