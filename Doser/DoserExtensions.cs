namespace IQbx.Doser
{
    using System;

    public static class DoserExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }

        public static T GetService<T>(this Doser serviceProvider, object key)
        {
            return (T)serviceProvider.Get(typeof(T), key);
        }

        public static Doser AddSingleton<TInterface, TImplementation>(this Doser doser) 
            where TImplementation : TInterface
        {
            doser.Add(typeof(TInterface), typeof(TImplementation), InstanceLifetime.Global);
            return doser;
        }

        public static Doser AddSingleton<TImplementation>(this Doser doser) where TImplementation : class
        {
            doser.Add(typeof(TImplementation), typeof(TImplementation), InstanceLifetime.Global);
            return doser;
        }

        public static Doser AddSingleton<TInterface, TImplementation>(this Doser doser, object key)
            where TImplementation : TInterface
        {
            doser.Add(typeof(TInterface), typeof(TImplementation), key, InstanceLifetime.Global);
            return doser;
        }

        public static Doser AddSingleton<TInterface>(this Doser doser, TInterface instance)
        {
            doser.Add<TInterface, TInterface>(() => instance, InstanceLifetime.Global);
            return doser;
        }

        public static Doser AddSingleton<TInterface>(this Doser doser, Func<TInterface> instanceFactory)
        {
            doser.Add<TInterface, TInterface>(instanceFactory, InstanceLifetime.Global);
            return doser;
        }

        public static Doser AddScoped<TInterface, TImplementation>(this Doser doser)
            where TImplementation : TInterface
        {
            doser.Add(typeof(TInterface), typeof(TImplementation), InstanceLifetime.Scoped);
            return doser;
        }

        public static Doser AddScoped<TInterface, TImplementation>(this Doser doser, object key)
            where TImplementation : TInterface
        {
            doser.Add(typeof(TInterface), typeof(TImplementation), key, InstanceLifetime.Scoped);
            return doser;
        }

        public static Doser AddScoped<TImplementation>(this Doser doser) where TImplementation : class
        {
            doser.Add(typeof(TImplementation), typeof(TImplementation), InstanceLifetime.Scoped);
            return doser;
        }

        public static Doser AddTransient<TInterface, TImplementation>(this Doser doser)
            where TImplementation : TInterface
        {
            doser.Add(typeof(TInterface), typeof(TImplementation), InstanceLifetime.Local);
            return doser;
        }

        public static Doser AddTransient<TImplementation>(this Doser doser) where TImplementation : class
        {
            doser.Add(typeof(TImplementation), typeof(TImplementation), InstanceLifetime.Local);
            return doser;
        }

        public static Doser AddTransient<TInterface, TImplementation>(this Doser doser, object key)
            where TImplementation : TInterface
        {
            doser.Add(typeof(TInterface), typeof(TImplementation), key, InstanceLifetime.Local);
            return doser;
        }
    }
}