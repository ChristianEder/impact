using System;
using Impact.Core.Matchers;

namespace Impact.Consumer
{
    public class ArrayMatcher<TItem>
    {
        private readonly string property;
        private readonly Action<IMatcher> register;

        public ArrayMatcher(string property, Action<IMatcher> register)
        {
            this.property = property;
            this.register = register;
        }

        public ArrayMatcher<TItem> LengthMin(long min)
        {
            register(new TypeMinPropertyMatcher(property, min));
            return this;
        }

        public ArrayMatcher<TItem> LengthMax(long max)
        {
            register(new TypeMaxPropertyMatcher(property, max));
            return this;
        }

        public ArrayMatcher<TItem> Length(long length)
        {
            register(new TypeMinPropertyMatcher(property, length));
            register(new TypeMaxPropertyMatcher(property, length));
            return this;
        }

        public PropertyMatcher<TItem> At(int index)
        {
            return new PropertyMatcher<TItem>($"{property}[{index}]", register);
        }

        public PropertyMatcher<TItem> All()
        {
            return new PropertyMatcher<TItem>($"{property}[*]", register);
        }
    }
}