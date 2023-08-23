using SqlComplexOperations.Attributes;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SqlComplexOperations.Extensions
{
    internal static class ExpressionExtension
    {
        public static List<string> GetMemberNames<T>(bool propNameAttr, params Expression<Func<T, object>>[] expressions)
        {
            if (expressions.Length == 1 && IsAnonymousType(expressions.First().Body.Type))
            {
                var exp = expressions.First();
                if(!propNameAttr)
                    return exp.Body.Type.GetProperties().Select(m => m.Name).ToList();

                var type = exp.Type.GenericTypeArguments.First(x => x.FullName == typeof(T).FullName);
                var allProps = type.GetProperties().ToList();
                var bodyExpProps = exp.Body.Type.GetProperties().Select(m => m.Name).ToList();

                return allProps.Where(x => bodyExpProps.Contains(x.Name)).Select(x => x.GetPropName(true)).ToList();

            }
            else if(expressions.Length == 1 && expressions.First().Body.Type == typeof(T))
            {
                var properties = expressions.First().Body.Type.GetProperties();
                properties = properties.Where(x => !x.GetGetMethod()?.IsVirtual ?? false).ToArray();
                
                return properties.Select(x => x.GetPropName(propNameAttr)).ToList();
            }

            return expressions.Select(expression => GetMemberName(expression.Body, propNameAttr)).ToList();
        }

        private static string GetMemberName(Expression expression, bool propNameAttr)
        {
            return expression switch
            {
                MemberExpression memberExpression => memberExpression.Member.GetMemberName(propNameAttr),
                MethodCallExpression methodCallExpression => methodCallExpression.Method.GetMethodName(propNameAttr),
                UnaryExpression unaryExpression => GetMemberName(unaryExpression, propNameAttr),
                _ => throw new ArgumentException($"The expression type is not supported ({nameof(expression)}).")
            };
        }

        private static string GetMemberName(UnaryExpression unaryExpression, bool propNameAttr)
        {
            if (unaryExpression.Operand is MethodCallExpression methodExpression)
                return methodExpression.Method.GetMethodName(propNameAttr);

            return ((MemberExpression)unaryExpression.Operand).Member.GetMemberName(propNameAttr);
        }

        private static bool IsAnonymousType(Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName != null && type.FullName.Contains("AnonymousType");
            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }
    }

    internal static class MemeberInfoExtension
    {
        public static string GetMemberName(this MemberInfo member, bool propNameAttr)
        {
            if(!propNameAttr)
                return member.Name;

            var attr = member.GetCustomAttribute<PropertyNameAttribute>(true);

            if(attr == null)
                return string.Empty;

            return attr.PropertyName;
        }
    }

    internal static class MethodInfoExtension
    {
        public static string GetMethodName(this MethodInfo methodInfo, bool propNameAttr)
        {
            if(!propNameAttr)
                return methodInfo.Name;

            var attr = methodInfo.GetCustomAttribute<PropertyNameAttribute>(true);

            if(attr == null)
                return string.Empty;

            return attr.PropertyName;
        }
    }
}
