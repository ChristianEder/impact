using Newtonsoft.Json.Linq;

namespace Impact.Consumer.Serialize
{
    public interface IRequestResponseSerializer
    {
        JToken Serialize(object o);
    }
}