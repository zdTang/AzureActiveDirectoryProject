using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AzureADB2CWeb.Extensions
{
    public static class SessionExtensions
    {
        public static T? GetComplexData<T>(this ISession session, string key)
        {
            var data = session.GetString(key);
            if (data == null)
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(data);
        }


        // Do we need using the T here ? I prefer deleting it !!!
        //public static void SetComplexData<T>(this ISession session, string key,object value)
        public static void SetComplexData(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
    }
}
