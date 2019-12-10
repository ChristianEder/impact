using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Impact.Core.Matchers;

namespace Impact.Consumer.Define
{
    public class PropertyMatcher<TProperty>
    {
        private readonly string propertyPath;
        private readonly Action<IMatcher> register;

        internal PropertyMatcher(string propertyPath, Action<IMatcher> register)
        {
            this.propertyPath = propertyPath;
            this.register = register;
        }

        public PropertyMatcher<TProperty> Type()
        {
            register(new TypePropertyMatcher(propertyPath));
            return this;
        }

        public PropertyMatcher<TProperty> TypeMax(long max)
        {
            register(new TypeMaxPropertyMatcher(propertyPath, max));
            return this;
        }

        public PropertyMatcher<TProperty> TypeMin(long min)
        {
            register(new TypeMinPropertyMatcher(propertyPath, min));
            return this;
        }

        public PropertyMatcher<TProperty> Regex(string regex)
        {
            register(new RegexPropertyMatcher(regex, propertyPath));
            return this;
        }

        public PropertyMatcher<TProperty> With<TSubProperty>(Expression<Func<TProperty, TSubProperty>> property, Action<PropertyMatcher<TSubProperty>> rule)
        {
            var matcher = new PropertyMatcher<TSubProperty>(propertyPath + "." + ExpressionToPropertyPath.Convert(property), register);
            rule(matcher);
            return this;
        }

        public PropertyMatcher<TProperty> WithArray<TItemProperty>(Expression<Func<TProperty, IEnumerable<TItemProperty>>> property, Action<ArrayMatcher<TItemProperty>> rules)
        {
            var matcher = new ArrayMatcher<TItemProperty>(propertyPath + "." + ExpressionToPropertyPath.Convert(property), register);
            rules(matcher);
            return this;
        }

    }
}