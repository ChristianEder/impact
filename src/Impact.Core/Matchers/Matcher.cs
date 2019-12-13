using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public abstract class Matcher : IMatcher
    {
        protected Matcher(string path)
        {
            PropertyPath = path;

            PropertyPathParts = path.Split(".").SelectMany(p =>
                p.EndsWith("]") ? ParseArrayPathParts(p) : Enumerable.Repeat(new PropertyPathPart(p), 1)
            ).ToArray();
        }

        private IEnumerable<IPropertyPathPart> ParseArrayPathParts(string part)
        {
            var open = part.IndexOf("[", StringComparison.Ordinal);
            var index = part.Substring(open + 1).TrimEnd(']');
            var name = part.Substring(0, open);

            yield return new PropertyPathPart(name);
            yield return new ArrayIndexPathPart(index);
        }

        public abstract bool Matches(object expected, object actual);

        public string PropertyPath { get; }

        public IPropertyPathPart[] PropertyPathParts { get; }

        public abstract JObject ToPactMatcher();

        public abstract IMatcher Clone(string propertyPath);

        public abstract string FailureMessage(object expected, object actual);
        public bool AppliesTo(List<IPropertyPathPart> propertyPath)
        {
            if (propertyPath.Count != PropertyPathParts.Length)
            {
                return false;
            }

            for (int i = 0; i < PropertyPathParts.Length; i++)
            {
                if (!PropertyPathParts[i].Equals(propertyPath[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}