namespace Doser.Implementation.Generic
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Exceptions;

    internal static class LazyResolver
    {
        public static IObjectResolver TryCreateLazyResolver(Type type,
            ResolverRepository typeResolvers, object key)
        {
            return InternalTryCreateLazyResolver(type, typeResolvers, resolver => resolver.GetResolver(key));
        }

        public static IObjectResolver TryCreateLazyResolver(Type type,
            ResolverRepository typeResolvers)
        {
            return InternalTryCreateLazyResolver(type, typeResolvers, resolver => resolver.GetResolver());
        }

        private static IObjectResolver InternalTryCreateLazyResolver(Type type,
            ResolverRepository typeResolvers, Func<TypeResolver, IObjectResolver> getResolver)
        {
            if (!type.IsInheritedFrom(typeof(Lazy<>)))
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
            var callExpression = resolver.CreateResolveExpression(innerType);

            var innerLambdaType = typeof(Func<>).MakeGenericType(innerType);
            var innerLambda = Expression.Lambda(innerLambdaType, callExpression);

            var lazyConstructor = type.GetConstructor(new[] { innerLambdaType });
            if (lazyConstructor == null)
            {
                throw new Exception($"Could not find constructor for {type.FullName}");
            }

            var lazyObject = Expression.New(lazyConstructor, innerLambda);
            return Expression.Lambda<Func<object>>(lazyObject).Compile();
        }
    }
}