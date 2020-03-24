using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impact.Consumer
{
    internal static class ExpressionToPropertyPath
    {
        public static string Convert<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            var memberExpression = GetMemberExpression(property.Body);
            var names = new Stack<string>();

            while (memberExpression != null)
            {
                names.Push(memberExpression.Member.Name);
                memberExpression = GetMemberExpression(memberExpression.Expression);
            }

            return string.Join(".", names);
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression is MemberExpression member)
            {
                return member;
            }

            if (expression is UnaryExpression unary)
            {
                return GetMemberExpression(unary.Operand);
            }

            return null;
        }
    }
}