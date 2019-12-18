using System;
using System.Collections;
using System.Linq;

namespace Impact.Core.Matchers
{
    public abstract class TypeCountMatcher : Matcher, IArrayLengthMatcher
    {
        protected TypeCountMatcher(string path) : base(path)
        {
            
        }

        protected long? Count(object actual, bool roundUp)
        {
            if (actual is IEnumerable e)
            {
                return e.Cast<object>().Count();
            }

            if (actual is int i)
            {
                return i;
            }
            if (actual is long l)
            {
                return l;
            }
            if (actual is decimal d)
            {
                return (long)(roundUp ? Math.Ceiling(d) : Math.Floor(d));
            }
            if (actual is float f)
            {
                return (long)(roundUp ? Math.Ceiling(f) : Math.Floor(f));
            }
            if (actual is double doub)
            {
                return (long)(roundUp ? Math.Ceiling(doub) : Math.Floor(doub));
            }

            return null;
        }
    }
}