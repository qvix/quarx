namespace Doser.Implementation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

using Exceptions;

internal class ObjectBuilder : IObjectResolver
{
    private readonly Type targetType;
    private readonly ResolverRepository resolvers;
    private Func<object>? creationFunction;
    public List<object> Constants = new ();
    private static readonly FieldInfo constantsField = typeof(ObjectBuilder).GetField(nameof(ObjectBuilder.Constants))!;
    
    public ObjectBuilder(Type targetType, ResolverRepository resolvers)
    {
        this.targetType = targetType;
        this.resolvers = resolvers;
    }

    public InstanceLifetime Lifetime => InstanceLifetime.Local;

    public object Resolve()
    {
        return (this.creationFunction ??= this.GetCreationFunction())();
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
            .Select<ParameterInfo, Expression>(item =>
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

                if (resolver.Lifetime == InstanceLifetime.Global)
                {
                    return Expression.Constant(resolver.Resolve());
                }

                var resolverMethod = resolver.Resolve;
                var methodInfo = resolverMethod.GetMethodInfo();

                return Expression.Convert(Expression.Call(Expression.Constant(resolverMethod.Target), methodInfo), parameterType);
            });

        return (Func<object>)Expression.Lambda(Expression.New(constructor, parameters)).Compile();
    }

    private Func<object> GetCreationFunctionIl()
    {
        if (this.targetType.IsInterface)
        {
            throw new Exception($"Cannot construct interface {this.targetType.Name}");
        }

        if (this.targetType.IsAbstract)
        {
            throw new Exception($"Cannot construct abstract class {this.targetType.Name}");
        }

        var method = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(object), Type.EmptyTypes, this.GetType(), true);
        var generator = method.GetILGenerator();

        var constructor = this.GetConstructorInfo();

        foreach (var parameter in constructor.GetParameters())
        {
            var parameterType = parameter.ParameterType;

            var typeResolver = this.resolvers.GetResolver(parameterType);
            var dependencyAttribute = Attribute.GetCustomAttribute(parameter, typeof(DependencyAttribute)) as DependencyAttribute;
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

            var resolverMethod = resolver.Resolve;
            var methodInfo = resolverMethod.GetMethodInfo();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, constantsField);

            generator.Emit(OpCodes.Ldc_I4, this.Constants.Count);
            generator.Emit(OpCodes.Ldelem, typeof(object));
            this.Constants.Add(resolver);

            generator.EmitCall(OpCodes.Callvirt, methodInfo, null);
            generator.Emit(OpCodes.Castclass, parameterType);
            generator.Emit(OpCodes.Stloc);
        }

        generator.Emit(OpCodes.Newobj, constructor);
        generator.Emit(OpCodes.Ret);

        return (Func<object>)method.CreateDelegate(typeof(Func<object>));

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