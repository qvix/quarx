namespace DoserBenchmark
{
    using System;
    using System.Runtime.CompilerServices;
    using BenchmarkDotNet.Attributes;
    using Doser;

    public class ScopeValidationBenchmark
    {
        private const int OperationsPerInvoke = 50000;

        private IServiceProvider staticSp;
        private IServiceProvider transientSp;
        private IServiceProvider scopeSp;
        private IScopeService scopeService;

        [GlobalSetup]
        public void Setup()
        {
            this.staticSp = new DoserProvider()
                .AddSingleton<A>()
                .AddSingleton<B>()
                .AddSingleton<C>()
                .Build();

            this.transientSp =new DoserProvider()
                .AddTransient<A>()
                .AddTransient<B>()
                .AddTransient<C>()
                .Build();

            this.scopeSp = new DoserProvider()
                .AddScoped<A>()
                .AddScoped<B>()
                .AddScoped<C>()
                .Build();
            this.scopeService = this.scopeSp.GetService<IScopeService>();
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Singleton()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = staticSp.GetService<A>();
                temp.Foo();
            }
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
        public void Transient()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = transientSp.GetService<A>();
                temp.Foo();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void TransientWithScopeValidation()
        {
            using (this.scopeService.CreateScope())
            {
                for (int i = 0; i < OperationsPerInvoke; i++)
                {
                    var temp = scopeSp.GetService<A>();
                    temp.Foo();
                }
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
}
