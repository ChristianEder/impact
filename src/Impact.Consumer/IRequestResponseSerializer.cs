using Newtonsoft.Json.Linq;

namespace Impact.Consumer
{
    public interface IRequestResponseSerializer
    {
        JToken Serialize(object o);
    }
}