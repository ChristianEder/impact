using System.Collections.Generic;

namespace Impact.Tests.Shared
{
    public class Request
    {
        public string Type { get; set; }

        public List<string> Ids { get; } = new List<string>();
    }
}