//using Microsoft.EntityFrameworkCore.Infrastructure;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace Bulk
//{
//    public class MergeBuilderCopy<TEntity> where TEntity : class
//    {
//        public List<string> MergeColumns { get; set; }

//        public MergeBuilder<TEntity> SetMergeColumns(params Expression<Func<TEntity, object>>[] expressions)
//        {
//            foreach (var expression in expressions)
//            {
//                var memberExpression = (MemberExpression)expression.Body;
//                var fieldName = memberExpression.Member.Name;

//                if (!string.IsNullOrWhiteSpace(fieldName))
//                    MergeColumns.Add(fieldName);

//            }


//            //if (GetColumns(out var columns, expressions))
//            //{
//            //    SetMergeColumns(columns.ToArray());
//            //}

//            return this;
//        }

//        public MergeBuilder<TEntity> SetMergeColumns(params string[] columns)
//        {
//            if (columns.Length == 0)
//                return this;

//            MergeColumns = columns.ToList();

//            return this;
//        }

//        private static bool GetColumns(out string[] columns, params Expression<Func<TEntity, object>>[] expressions)
//        {
//            columns = null;
//            var names = GetMemberNames(expressions);
//            if (names != null && names.Any())
//            {
//                columns = names.ToArray();
//            }

//            return columns != null && columns.Any();
//        }

//        public static List<string> GetMemberNames<T>(params Expression<Func<T, object>>[] expressions)
//        {
//            if (expressions.Length == 1 && IsAnonymousType(expressions.First().Body.Type))
//            {
//                return expressions.First().Body.Type.GetProperties().Select(m => m.Name).ToList();
//            }

//            return expressions.Select(expression => GetMemberName(expression.Body)).ToList();
//        }

//        public static bool IsAnonymousType(Type type)
//        {
//            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
//            var nameContainsAnonymousType = type.FullName != null && type.FullName.Contains("AnonymousType");
//            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
//        }

//        private static string GetMemberName(Expression expression)
//        {
//            return expression switch
//            {
//                null => throw new ArgumentException(""),
//                MemberExpression memberExpression => memberExpression.Member.Name,
//                MethodCallExpression methodCallExpression => methodCallExpression.Method.Name,
//                UnaryExpression unaryExpression => GetMemberName(unaryExpression),
//                _ => throw new ArgumentException(""),
//            };
//        }

//        private static string GetMemberName(UnaryExpression unaryExpression)
//        {
//            if (unaryExpression.Operand is MethodCallExpression methodExpression)
//            {
//                return methodExpression.Method.Name;
//            }

//            return ((MemberExpression)unaryExpression.Operand).Member.Name;
//        }
//        public MergeBuilder<TEntity> SetCondition(params Expression<Func<TEntity, (object, ConditionTypes)>>[] expressions)
//        {
//            foreach (var expression in expressions)
//            {
//                var memberExpression = (MemberExpression)expression.Body;
//                var fieldName = memberExpression.Member.Name;

//                if (!string.IsNullOrWhiteSpace(fieldName))
//                    MergeColumns.Add(fieldName);

//            }

//            return this;
//        }

//    }

//    public class ConditionalClass
//    {
//        public object obj { get; set; }
//        public ConditionTypes conditional { get; set; }
//    }
//}
