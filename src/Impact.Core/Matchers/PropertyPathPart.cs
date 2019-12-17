using System;

namespace Impact.Core.Matchers
{
    public class PropertyPathPart : IPropertyPathPart
    {
        public PropertyPathPart(string value, bool ignoreCase = false)
        {
            Value = value;
        }

        public bool Equals(IPropertyPathPart other)
        {
            var otherProperty = other as PropertyPathPart;
            if (ReferenceEquals(otherProperty, null))
            {
                return false;
            }

            return string.Equals(Value, otherProperty.Value, StringComparison.InvariantCultureIgnoreCase) || Value == "*" || otherProperty?.Value == "*";
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}