using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace OctanificationServer.StaticData
{
    public interface IJSONable { }

    public static class JSONableExtensions
    {
        public static string ToJson<T>(this T self) where T : IJSONable
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                ser.WriteObject(ms, self);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static string ToJsonArray<T>(this T[] self) where T : IJSONable
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T[]));
            using (MemoryStream ms = new MemoryStream())
            {
                ser.WriteObject(ms, self);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T FromJson<T>(Stream json) where T : IJSONable
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            return (T)ser.ReadObject(json);
        }

        public static T FromJson<T>(string json) where T : IJSONable
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return FromJson<T>(ms);
            }
        }

        public static T[] FromJsonArray<T>(Stream json) where T : IJSONable
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T[]));
            return (T[])ser.ReadObject(json);
        }

        public static T[] FromJsonArray<T>(string json) where T : IJSONable
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return FromJsonArray<T>(ms);
            }
        }

    }
}
