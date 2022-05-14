namespace Doser.Implementation.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Exceptions;
    
    internal static class EnumerableResolver
    {
        private static readonly MethodInfo ToArrayGeneric = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));
        private static readonly MethodInfo GetObjectsMethod = typeof(EnumerableResolver).GetMethod(nameof(GetObjects), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo CastMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast));

        public static IObjectResolver TryCreateEnumerableResolver(Type type, ResolverRepository typeResolvers)
        {
            if (!type.IsInterfaceImplementor(typeof(IEnumerable<>)))
            {
                return null;
            }

            var innerType = type.IsArray && type.GetArrayRank() == 1
                ? type.GetElementType()
                : type.GetGenericArguments().FirstOrDefault();
            if (innerType == null)
            {
                throw new ArgumentException($"Cannot create resolver for {type.FullName}");
            }

            if (!typeResolvers.TryGetValue(innerType, out var targetResolver))
            {
                throw new ResolveException(innerType);
            }

            targetResolver.Build();
            var enumerableCastFunc = CreateLambda(type, innerType, targetResolver.GetResolvers());

            return new InstanceFactory(enumerableCastFunc, InstanceLifetime.Local);
        }

        private static Func<object> CreateLambda(Type targetType, Type type, IObjectResolver[] resolvers)
        {
            var enumerableGenericType = typeof(IEnumerable<>);
            var enumerableTargetType = enumerableGenericType.MakeGenericType(type);
            var castMethod = CastMethod.MakeGenericMethod(type);
            var results = new object[resolvers.Length];

            var callExpression = Expression.Convert(
                Expression.Call(castMethod, Expression.Call(GetObjectsMethod, Expression.Constant(resolvers), Expression.Constant(results))),
                enumerableTargetType);

            return Expression.Lambda<Func<object>>(GetTargetObject(targetType, type, callExpression, enumerableTargetType)).Compile();
        }

        private static IEnumerable<object> GetObjects(IObjectResolver[] resolvers, object[] results)
        {
            for (int i = 0; i < resolvers.Length; i++)
            {
                results[i] = resolvers[i].Get();
            }
            return results;
        }

        private static Expression GetTargetObject(Type targetType, Type innerType, Expression enumerableSource, Type enumerableTargetType)
        {
            if (targetType == enumerableTargetType)
            {
                return enumerableSource;
            }

            if (targetType.IsArray)
            {
                var toArrayMethod = ToArrayGeneric.MakeGenericMethod(innerType);

                return Expression.Call(toArrayMethod, enumerableSource);
            }

            if (targetType.IsInterfaceImplementor(typeof(ICollection<>)))
            {
                var genericList = typeof(List<>);
                var targetList = genericList.MakeGenericType(innerType);
                var listConstructor = targetList.GetConstructor(new[] { enumerableTargetType });
                if (listConstructor == null)
                {
                    throw new ResolveException($"Could not find constructor for List<{targetType.FullName}>");
                }

                return Expression.New(listConstructor, enumerableSource);
            }

            throw new ResolveException($"Could not find constructor target type {targetType.FullName}");
        }

    }
}