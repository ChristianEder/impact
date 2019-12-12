using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Impact.Core.Matchers
{
    public abstract class Matcher : IMatcher
    {
        private readonly IPropertyPathPart[] pathParts;

        protected Matcher(string path)
        {
            PropertyPath = path;
            pathParts = path.Split(".").Select(p => p.EndsWith("]") ? (IPropertyPathPart)new ArrayPropertyPathPart(p) : new PropertyPathPart(p)).ToArray();
        }

        public abstract bool Matches(object expected, object actual);

        public string PropertyPath { get; }
        public IPropertyPathPart[] PropertyPathParts => pathParts;
        public abstract JObject ToPactMatcher();

        public abstract IMatcher Clone(string propertyPath);

        public abstract string FailureMessage(object expected, object actual);
        public bool AppliesTo(List<IPropertyPathPart> propertyPath)
        {
            if (propertyPath.Count != pathParts.Length)
            {
                return false;
            }

            for (int i = 0; i < pathParts.Length; i++)
            {
                if (!pathParts[i].Equals(propertyPath[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}