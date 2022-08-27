namespace Doser.Implementation;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Exceptions;

internal class ObjectBuilder : IObjectResolver
{
    private readonly Type targetType;
    private readonly ResolverRepository resolvers;
    private Func<object> creationFunction;

    public ObjectBuilder(Type targetType, ResolverRepository resolvers)
    {
        this.targetType = targetType;
        this.resolvers = resolvers;
    }

    public InstanceLifetime Lifetime => InstanceLifetime.Local;

    public Func<object> GetResolver()
    {
        return this.creationFunction ??= this.GetCreationFunction();
    }

    public void Build()
    {
        this.creationFunction ??= this.GetCreationFunction();
    }

    private Func<object> GetCreationFunction()
    {
        if (this.targetType.IsInterface)
        {
            throw new Exception($"Cannot construct interface {this.targetType.Name}");
        }

        if (this.targetType.IsAbstract)
        {
            throw new Exception($"Cannot construct abstract class {this.targetType.Name}");
        }

        var constructor = this.GetConstructorInfo();
        var parameters = constructor.GetParameters()
            .Select(item =>
            {
                var parameterType = item.ParameterType;

                var typeResolver = this.resolvers.GetResolver(parameterType);
                var dependencyAttribute = Attribute.GetCustomAttribute(item, typeof(DependencyAttribute)) as DependencyAttribute;
                var resolver = dependencyAttribute == null
                    ? typeResolver.GetResolver() 
                    : typeResolver.GetResolver(dependencyAttribute.Key);

                if (resolver == null)
                {
                    if (dependencyAttribute == null)
                    {
                        throw new ResolveException(this.targetType);
                    }
                    throw new ResolveException(this.targetType, dependencyAttribute.Key);
                }

                resolver.Build();

                return resolver.CreateResolveExpression(parameterType);
            });

        return (Func<object>)Expression.Lambda(Expression.New(constructor, parameters)).Compile();
    }

    private ConstructorInfo GetConstructorInfo()
    {
        var constructors = this.targetType.GetConstructors();
        return constructors.Length switch
        {
            0 => throw new Exception($"Type {this.targetType.FullName} has no constructors"),
            1 => constructors[0],
            _ => throw new Exception($"Type {this.targetType.FullName} has no suitable constructor")
        };
    }
}