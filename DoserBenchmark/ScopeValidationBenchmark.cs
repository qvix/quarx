namespace DoserBenchmark
{
    using System;
    using System.Runtime.CompilerServices;
    using BenchmarkDotNet.Attributes;
    using Doser;

    public class ScopeValidationBenchmark
    {
        private const int OperationsPerInvoke = 50000;

        private IServiceProvider _transientSp;
        private IServiceProvider _transientSpScopeValidation;

        [GlobalSetup]
        public void Setup()
        {
            var services = new DoserProvider();
            services.AddTransient<A>();
            services.AddTransient<B>();
            services.AddTransient<C>();
            _transientSp = services;

            services = new DoserProvider();
            services.AddTransient<A>();
            services.AddTransient<B>();
            services.AddTransient<C>();
            _transientSpScopeValidation = services;
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
        public void Transient()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _transientSp.GetService<A>();
                temp.Foo();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void TransientWithScopeValidation()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _transientSpScopeValidation.GetService<A>();
                temp.Foo();
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
