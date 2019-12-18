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

            if (index.StartsWith("'") && index.EndsWith("'"))
            {
                yield return new PropertyPathPart(index.Trim('\''));
            }
            else
            {
                yield return new ArrayIndexPathPart(index);
            }
        }

        public abstract bool Matches(object expected, object actual, MatchingContext context, Action<object, object, MatchingContext> deepMatch);

        public string PropertyPath { get; }

        public IPropertyPathPart[] PropertyPathParts { get; }

        public abstract JObject ToPactMatcher();

        public abstract string FailureMessage(object expected, object actual);
        public virtual bool AppliesTo(List<IPropertyPathPart> propertyPath)
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

        public virtual bool AppliesTo(object expected, object actual, MatchingContext context)
        {
            return true;
        }

        public virtual bool IsTerminal => true;
    }
}