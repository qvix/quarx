namespace Doser.Implementation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;

    internal class ObjectBuilder : IObjectResolver
    {
        private static readonly MethodInfo GetObjectMethod = typeof(ObjectResolver).GetMethod(nameof(ObjectResolver.Get));

        private readonly Type targetType;
        private readonly ResolverRepository resolvers;
        private Func<object> creationFunction;

        public ObjectBuilder(Type targetType, ResolverRepository resolvers)
        {
            targetType.CheckNotNull();

            this.targetType = targetType;
            this.resolvers = resolvers;
        }

        public Func<object> Resolve(Func<object> next)
        {
            return LazyInitializer.EnsureInitialized(ref this.creationFunction, this.GetCreationFunction);
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

                    var typeResolver = this.resolvers[parameterType];
                    var resolver = Attribute.GetCustomAttribute(item, typeof(DependencyAttribute)) is not DependencyAttribute dependencyAttribute 
                        ? typeResolver.GetResolver() 
                        : typeResolver.GetResolver(dependencyAttribute.Key);
                    var instance = Expression.Constant(resolver);

                    return Expression.Convert(Expression.Call(instance, GetObjectMethod), parameterType);
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
}