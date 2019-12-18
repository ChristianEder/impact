namespace Impact.Core.Matchers
{
    public class ArrayIndexPathPart : IPropertyPathPart
    {
        private readonly bool exact;

        public ArrayIndexPathPart(string value, bool exact = false)
        {
            this.exact = exact;
            Value = value;
        }

        public bool Equals(IPropertyPathPart other)
        {
            var otherArray = other as ArrayIndexPathPart;
            if (ReferenceEquals(otherArray, null))
            {
                return false;
            }

            if (exact || otherArray.exact)
            {
                return Value == otherArray.Value;
            }
            
            return Value == otherArray.Value || Value == "*" || otherArray.Value == "*";
        }

        public string Value { get; }

        public override string ToString()
        {
            return $"[{Value}]";
        }
    }
}