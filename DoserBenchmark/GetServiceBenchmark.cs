namespace IQbx.DoserBenchmark
{
/*

    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using BenchmarkDotNet.Attributes;
    using IQbx.Doser;

    public class GetServiceBenchmark
    {
        private const int OperationsPerInvoke = 50000;

        private IServiceProvider _transientSp;
        private IServiceProvider _singletonSp;
        private IServiceProvider _scopeSp;
        private IServiceProvider _serviceScopeFactoryProvider;
        private IServiceProvider _emptyEnumerable;

        // private IServiceScope _scopedSp;


        [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
        public void NoDI()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = new A(new B(new C()));
                temp.Foo();
            }
        }

        [GlobalSetup(Target = nameof(Transient))]
        public void SetupTransient()
        {
            _singletonSp = new Doser()
                .AddTransient<A>()
                .AddTransient<B>()
                .AddTransient<C>();
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Transient()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = this._transientSp.GetService<A>();
                temp.Foo();
            }
        }

        [GlobalSetup(Target = nameof(Scoped))]
        public void SetupScoped()
        {
            _scopedSp = new Doser()
                .AddScoped<A>()
                .AddScoped<B>()
                .AddScoped<C>();
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Scoped()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _scopedSp.ServiceProvider.GetService<A>();
                temp.Foo();
            }
        }

        [GlobalSetup(Target = nameof(Singleton))]
        public void SetupScopedSingleton()
        {
            _singletonSp = new Doser()
                .AddSingleton<A>()
                .AddSingleton<B>()
                .AddSingleton<C>();
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Singleton()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _singletonSp.GetService<A>();
                temp.Foo();
            }
        }

        [GlobalSetup(Target = nameof(ServiceScope))]
        public void ServiceScopeSetup()
        {
            _scopeSp = new ServiceCollection().BuildServiceProvider(new ServiceProviderOptions()
            {
                Mode = _mode
            });
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void ServiceScope()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _scopeSp.CreateScope();
            }
        }

        [GlobalSetup(Target = nameof(ServiceScopeProvider))]
        public void ServiceScopeProviderSetup()
        {
            _serviceScopeFactoryProvider = new ServiceCollection().BuildServiceProvider(new ServiceProviderOptions()
            {
                Mode = _mode
            });
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void ServiceScopeProvider()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _serviceScopeFactoryProvider.GetService<IServiceScopeFactory>();
            }
        }

        [GlobalSetup(Target = nameof(EmptyEnumerable))]
        public void EmptyEnumerableSetup()
        {
            _emptyEnumerable = new ServiceCollection().BuildServiceProvider(new ServiceProviderOptions()
            {
                Mode = _mode
            });
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void EmptyEnumerable()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                _emptyEnumerable.GetService<IEnumerable<A>>();
            }
        }

        private class A
        {
            public A(B b)
            {

            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void Foo()
            {

            }
        }

        private class B
        {
            public B(C c)
            {

            }
        }

        private class C
        {

        }
    }
*/
}
