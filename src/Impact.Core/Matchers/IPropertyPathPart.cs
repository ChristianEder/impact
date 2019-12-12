using System;

namespace Impact.Core.Matchers
{
    public interface IPropertyPathPart : IEquatable<IPropertyPathPart>
    {
        string Value { get; }
    }
}