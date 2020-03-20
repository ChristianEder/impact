using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impact.Consumer
{
    public static class ExpressionToPropertyPath
    {
        public static string Convert<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            var getMemberNameFunc = new Func<Expression, MemberExpression>(expression => expression as MemberExpression);
            var memberExpression = getMemberNameFunc(property.Body);
            var names = new Stack<string>();

            while (memberExpression != null)
            {
                names.Push(memberExpression.Member.Name);
                memberExpression = getMemberNameFunc(memberExpression.Expression);
            }

            return string.Join(".", names);
        }
    }
}