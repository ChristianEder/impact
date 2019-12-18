using Newtonsoft.Json.Linq;

namespace Impact.Core
{
    public static class JTokenExtensions
    {
        private static readonly JValue NullValue = JValue.CreateNull();

        public static JToken CreateEmpty(this JToken j)
        {
            switch (j)
            {
                case JValue _:
                    return JValue.CreateNull();
                case JObject _:
                    return new JObject();
                case JArray _:
                    return new JArray();
            }

            return null;
        }

        public static bool IsNullish(this JToken j)
        {
            return ReferenceEquals(null, j) || j is JValue v && v.Equals(NullValue);
        }
    }
}