using System.Web.Script.Serialization;

namespace UnityEngine
{
    public static class JsonUtility
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();
        public static T FromJson<T>(string json) => Serializer.Deserialize<T>(json);
    }
}
