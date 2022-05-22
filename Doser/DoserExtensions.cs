namespace Doser
{
    using System;

    public static class DoserExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }

        public static T GetService<T>(this IDoserServiceProvider serviceProvider, object key)
        {
            return (T)serviceProvider.GetService(typeof(T), key);
        }

        public static DoserProvider AddSingleton<TInterface, TImplementation>(this DoserProvider doser) 
            where TImplementation : TInterface
        {
            return doser.Add(typeof(TInterface), typeof(TImplementation), InstanceLifetime.Global);
        }

        public static DoserProvider AddSingleton<TImplementation>(this DoserProvider doser) where TImplementation : class
        {
            return doser.Add(typeof(TImplementation), typeof(TImplementation), InstanceLifetime.Global);
        }

        public static DoserProvider AddSingleton<TInterface, TImplementation>(this DoserProvider doser, object key)
            where TImplementation : TInterface
        {
            return doser.Add(typeof(TInterface), typeof(TImplementation), key, InstanceLifetime.Global);
        }

        public static DoserProvider AddSingleton<TInterface>(this DoserProvider doser, TInterface instance)
        {
            return doser.Add<TInterface, TInterface>(() => instance, InstanceLifetime.Global);
        }

        public static DoserProvider AddSingleton<TInterface>(this DoserProvider doser, Func<TInterface> instanceFactory)
        {
            return doser.Add<TInterface, TInterface>(instanceFactory, InstanceLifetime.Global);
        }

        public static DoserProvider AddScoped<TInterface, TImplementation>(this DoserProvider doser)
            where TImplementation : TInterface
        {
            return doser.Add(typeof(TInterface), typeof(TImplementation), InstanceLifetime.Scoped);
        }

        public static DoserProvider AddScopeTransparent<TInterface, TImplementation>(this DoserProvider doser)
            where TImplementation : TInterface
        {
            return doser.Add(typeof(TInterface), typeof(TImplementation), InstanceLifetime.ScopeTransparent);
        }

        public static DoserProvider AddScoped<TInterface, TImplementation>(this DoserProvider doser, object key)
            where TImplementation : TInterface
        {
            return doser.Add(typeof(TInterface), typeof(TImplementation), key, InstanceLifetime.Scoped);
        }

        public static DoserProvider AddScoped<TImplementation>(this DoserProvider doser) where TImplementation : class
        {
            return doser.Add(typeof(TImplementation), typeof(TImplementation), InstanceLifetime.Scoped);
        }

        public static DoserProvider AddTransient<TInterface, TImplementation>(this DoserProvider doser)
            where TImplementation : TInterface
        {
            return doser.Add(typeof(TInterface), typeof(TImplementation), InstanceLifetime.Local);
        }

        public static DoserProvider AddTransient<TImplementation>(this DoserProvider doser) where TImplementation : class
        {
            return doser.Add(typeof(TImplementation), typeof(TImplementation), InstanceLifetime.Local);
        }

        public static DoserProvider AddTransient<TInterface, TImplementation>(this DoserProvider doser, object key)
            where TImplementation : TInterface
        {
            return doser.Add(typeof(TInterface), typeof(TImplementation), key, InstanceLifetime.Local);
        }
    }
}