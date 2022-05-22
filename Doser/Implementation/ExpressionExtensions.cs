namespace Doser.Implementation
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ExpressionExtensions
    {
        public static Expression CreateResolveExpression(this IObjectResolver resolver, Type resultType)
        {
            var resolverMethod = resolver.GetResolver();
            var methodInfo = resolverMethod.GetMethodInfo();
            if (methodInfo.DeclaringType == null)
            {

                return Expression.Convert(Expression.Invoke(Expression.Constant(resolverMethod)), resultType);
            }

            return Expression.Convert(Expression.Call(Expression.Constant(resolverMethod.Target), methodInfo), resultType);
        }
    }
}