using System;

namespace Impact.Core.Matchers
{
    public class ArrayPropertyPathPart : IPropertyPathPart
    {
        private readonly string index;
        private readonly string name;

        public ArrayPropertyPathPart(string value)
        {
            Value = value;
            var open = value.IndexOf("[", StringComparison.Ordinal);
            index = value.Substring(open + 1).TrimEnd(']');
            name = value.Substring(0, open);
        }

        public bool Equals(IPropertyPathPart other)
        {
            var otherArray = other as ArrayPropertyPathPart;

            if (ReferenceEquals(otherArray, null))
            {
                return false;
            }



            if (Value == otherArray.Value)
            {
                return true;
            }

            if (name != otherArray.name)
            {
                return false;
            }
            
            return index == otherArray.index || index == "*" || otherArray.index == "*";
        }

        public string Value { get; }
    }
}