using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Bulk.Extensions
{
    internal class ExpressionExtension
    {
        public static List<string> GetMemberNames<T>(params Expression<Func<T, object>>[] expressions)
        {
            if (expressions.Length == 1 && IsAnonymousType(expressions.First().Body.Type))
            {
                return expressions.First().Body.Type.GetProperties().Select(m => m.Name).ToList();
            }
            else if(expressions.Length == 1 && expressions.First().Body.Type == typeof(T))
            {
                return expressions.First().Body.Type.GetProperties().Select(m => m.Name).ToList();
            }

            return expressions.Select(expression => GetMemberName(expression.Body)).ToList();
        }

        private static string GetMemberName(Expression expression)
        {
            return expression switch
            {
                null => throw new ArgumentException(""),
                MemberExpression memberExpression => memberExpression.Member.Name,
                MethodCallExpression methodCallExpression => methodCallExpression.Method.Name,
                UnaryExpression unaryExpression => GetMemberName(unaryExpression),
                _ => throw new ArgumentException(""),
            };
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression methodExpression)
                return methodExpression.Method.Name;

            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }

        private static bool IsAnonymousType(Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName != null && type.FullName.Contains("AnonymousType");
            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }
    }
}
