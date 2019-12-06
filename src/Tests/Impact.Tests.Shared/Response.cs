using System.Collections.Generic;

namespace Impact.Tests.Shared
{
    public class Response
    {
        public List<Foo> Foos { get; } = new List<Foo>();
        public List<Bar> Bars { get; } = new List<Bar>();
    }
}