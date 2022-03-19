namespace IQbx.Doser.Implementation.Generic
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Exceptions;

    internal static class FuncResolver
    {
        private static readonly MethodInfo ResolverGetMethod = typeof(ObjectResolver).GetMethod(nameof(ObjectResolver.Get));

        public static ObjectResolver TryCreateFuncResolver(Type type,
            ResolverRepository typeResolvers, object key)
        {
            return InternalTryCreateFuncResolver(type, typeResolvers, resolver => resolver.GetResolver(key));
        }

        public static ObjectResolver TryCreateFuncResolver(Type type,
            ResolverRepository typeResolvers)
        {
            return InternalTryCreateFuncResolver(type, typeResolvers, resolver => resolver.GetResolver());
        }

        private static ObjectResolver InternalTryCreateFuncResolver(Type type,
            ResolverRepository typeResolvers, Func<TypeResolver, ObjectResolver> getResolver)
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

            return new ObjectResolver(new IObjectResolver[] { new InstanceFactory(enumerableCastFunc) });
        }

        private static Func<object> CreateLambda(Type type, Type innerType, ObjectResolver resolver)
        {
            // new Func<object>(() => new Func<RealType>(resolver.Invoke()));
            var callExpression = Expression.Convert(Expression.Call(Expression.Constant(resolver), ResolverGetMethod), innerType);

            var innerLambda = Expression.Lambda(type, callExpression);
            return Expression.Lambda<Func<object>>(innerLambda).Compile();
        }
    }
}