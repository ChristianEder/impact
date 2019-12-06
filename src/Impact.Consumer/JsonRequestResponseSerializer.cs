using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Impact.Consumer
{
    public class JsonRequestResponseSerializer : IRequestResponseSerializer
    {
        public JToken Serialize(object o)
        {
            return JsonConvert.SerializeObject(o);
        }
    }
}