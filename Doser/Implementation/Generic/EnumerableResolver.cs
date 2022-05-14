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

        public static ObjectResolver TryCreateEnumerableResolver(Type type, ResolverRepository typeResolvers)
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

            var enumerableCastFunc = CreateLambda(type, innerType, targetResolver.GetResolvers());

            return new ObjectResolver(new InstanceFactory(enumerableCastFunc));
        }

        private static Func<object> CreateLambda(Type targetType, Type type, IEnumerable<ObjectResolver> resolvers)
        {
            // new Func<object>(() => new IEnumerable{RealType}(resolvers.Get()));
            var enumerableGenericType = typeof(IEnumerable<>);
            var enumerableTargetType = enumerableGenericType.MakeGenericType(type);
            var castMethod = CastMethod.MakeGenericMethod(type);

            var callExpression = Expression.Convert(
                Expression.Call(castMethod, Expression.Call(GetObjectsMethod, Expression.Constant(resolvers))),
                enumerableTargetType);

            return Expression.Lambda<Func<object>>(GetTargetObject(targetType, type, callExpression, enumerableTargetType)).Compile();
        }

        private static IEnumerable<object> GetObjects(IEnumerable<ObjectResolver> resolvers)
        {
            return resolvers.Select((x => x.Get()));
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