namespace Doser.Implementation.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    using Exceptions;

    internal static class LazyResolver
    {
        private static Type InnerType => typeof(Lazy<>);
        private static readonly MethodInfo ResolverGetMethod = typeof(ObjectResolver).GetMethod(nameof(ObjectResolver.Get));

        public static ObjectResolver TryCreateLazyResolver(Type type,
            ResolverRepository typeResolvers, object key)
        {
            return InternalTryCreateLazyResolver(type, typeResolvers, resolver => resolver.GetResolver(key));
        }

        public static ObjectResolver TryCreateLazyResolver(Type type,
            ResolverRepository typeResolvers)
        {
            return InternalTryCreateLazyResolver(type, typeResolvers, resolver => resolver.GetResolver());
        }

        private static ObjectResolver InternalTryCreateLazyResolver(Type type,
            ResolverRepository typeResolvers, Func<TypeResolver, ObjectResolver> getResolver)
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

            return new ObjectResolver(new IObjectResolver[] { new InstanceFactory(enumerableCastFunc) });
        }

        private static Func<object> CreateLambda(Type type, Type innerType, ObjectResolver resolver)
        {
            // new Func<IContainer, object>(container => () => new Func{RealType}(container.Resolve(innerType, key)));
            var callExpression = Expression.Convert(Expression.Call(Expression.Constant(resolver), ResolverGetMethod), innerType);

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