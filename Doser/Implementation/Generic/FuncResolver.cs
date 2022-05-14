namespace Doser.Implementation.Generic
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Exceptions;

    internal static class FuncResolver
    {
        private static readonly MethodInfo ResolverGetMethod = typeof(IObjectResolver).GetMethod(nameof(IObjectResolver.Get));

        public static IObjectResolver TryCreateFuncResolver(Type type,
            ResolverRepository typeResolvers, object key)
        {
            return InternalTryCreateFuncResolver(type, typeResolvers, resolver => resolver.GetResolver(key));
        }

        public static IObjectResolver TryCreateFuncResolver(Type type,
            ResolverRepository typeResolvers)
        {
            return InternalTryCreateFuncResolver(type, typeResolvers, resolver => resolver.GetResolver());
        }

        private static IObjectResolver InternalTryCreateFuncResolver(Type type,
            ResolverRepository typeResolvers, Func<TypeResolver, IObjectResolver> getResolver)
        {
            if (!type.IsInheritedFrom(typeof(Func<>)))
            {
                return null;
            }

            var genericArguments = type.GetGenericArguments();
            if (genericArguments.Length != 1)
            {
                throw new ArgumentException($"Cannot create resolver for {type.FullName}");
            }

            var innerType = genericArguments[0];

            if (!typeResolvers.TryGetValue(innerType, out var targetResolver))
            {
                throw new ResolveException(innerType);
            }

            var enumerableCastFunc = CreateLambda(type, innerType, getResolver(targetResolver));

            return new InstanceFactory(enumerableCastFunc, InstanceLifetime.Local);
        }

        private static Func<object> CreateLambda(Type type, Type innerType, IObjectResolver resolver)
        {
            // new Func<object>(() => new Func<RealType>(resolver.Invoke()));
            var callExpression = Expression.Convert(Expression.Call(Expression.Constant(resolver), ResolverGetMethod), innerType);

            var innerLambda = Expression.Lambda(type, callExpression);
            return Expression.Lambda<Func<object>>(innerLambda).Compile();
        }
    }
}