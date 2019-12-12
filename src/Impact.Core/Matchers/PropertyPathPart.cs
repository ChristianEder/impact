namespace Impact.Core.Matchers
{
    public class PropertyPathPart : IPropertyPathPart
    {
        public PropertyPathPart(string value)
        {
            Value = value;
        }

        public bool Equals(IPropertyPathPart other)
        {
            return Value == other?.Value || Value == "*" || other?.Value == "*";
        }

        public string Value { get; }
    }
}