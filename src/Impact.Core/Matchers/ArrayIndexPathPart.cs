namespace Impact.Core.Matchers
{
    public class ArrayIndexPathPart : IPropertyPathPart
    {
        public ArrayIndexPathPart(string value)
        {
            Value = value;
        }

        public bool Equals(IPropertyPathPart other)
        {
            var otherArray = other as ArrayIndexPathPart;
            if (ReferenceEquals(otherArray, null))
            {
                return false;
            }
            
            return Value == otherArray?.Value || Value == "*" || otherArray?.Value == "*";
        }

        public string Value { get; }

        public override string ToString()
        {
            return $"[{Value}]";
        }
    }
}