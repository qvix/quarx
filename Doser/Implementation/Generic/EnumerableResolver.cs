namespace Doser.Implementation.Generic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Exceptions;
    
internal static class EnumerableResolver
{
    private static readonly MethodInfo getObjectsMethod = typeof(EnumerableResolver).GetMethod(nameof(GetObjects), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static IObjectResolver? TryCreateEnumerableResolver(Type type, ResolverRepository typeResolvers)
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

        targetResolver!.Build();
        var enumerableCastFunc = CreateLambda(type, innerType, targetResolver.GetResolvers());

        return new InstanceFactory(enumerableCastFunc, InstanceLifetime.Local);
    }

    private static Func<object> CreateLambda(Type targetType, Type type, IObjectResolver[] resolvers)
    {
        var enumerableGenericType = typeof(IEnumerable<>);
        var enumerableSourceType = enumerableGenericType.MakeGenericType(type);
        var results = Array.CreateInstance(type, resolvers.Length);
        var getObjectsMethodTyped = getObjectsMethod.MakeGenericMethod(type);

        var getObjectsExpression = Expression.Call(getObjectsMethodTyped, Expression.Constant(resolvers), Expression.Constant(results));

        return Expression.Lambda<Func<object>>(GetTargetObject(targetType, type, getObjectsExpression, enumerableSourceType)).Compile();
    }

    private static T[] GetObjects<T>(IObjectResolver[] resolvers, T[] results)
    {
        for (int i = 0; i < resolvers.Length; i++)
        {
            results[i] = (T)resolvers[i].Resolve()!;
        }
        return results;
    }

    private static Expression GetTargetObject(Type targetType, Type innerType, Expression getObjectsExpression, Type enumerableSourceType)
    {
        if (targetType == enumerableSourceType)
        {
            return getObjectsExpression;
        }

        if (targetType.IsArray)
        {
            return getObjectsExpression;
        }

        if (targetType.IsInterfaceImplementor(typeof(ICollection<>)))
        {
            var genericList = typeof(List<>);
            var targetList = genericList.MakeGenericType(innerType);
            var listConstructor = targetList.GetConstructor(new[] { enumerableSourceType });
            if (listConstructor == null)
            {
                throw new ResolveException($"Could not find constructor for List<{targetType.FullName}>");
            }

            return Expression.New(listConstructor, getObjectsExpression);
        }

        throw new ResolveException($"Could not find constructor target type {targetType.FullName}");
    }
}