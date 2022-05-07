using Microsoft.Extensions.DependencyInjection;

namespace DoserBenchmark;

using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using Doser;
using Entities;
using System;

[MemoryDiagnoser]
public class ActivatorBenchmark
{
    private IDoserServiceProvider doserProvider;
    private IServiceProvider serviceProvider;
    private object[] factoryArguments;
    private DependencyA dependencyA;
    private DependencyB dependencyB;
    private DependencyC dependencyC;
    private IEnumerable<IData> data;

    [GlobalSetup]
    public void SetUp()
    {
        this.doserProvider = new DoserProvider()
            .AddTransient<TypeToBeActivated>()
            .AddSingleton<DependencyA>()
            .AddSingleton<DependencyB>()
            .AddSingleton<IData, DependencyA>()
            .AddSingleton<IData, DependencyB>()
            .AddSingleton<DependencyC>()
            .Build();

        this.serviceProvider = new ServiceCollection()
            .AddTransient<TypeToBeActivated>()
            .AddSingleton<DependencyA>()
            .AddSingleton<DependencyB>()
            .AddSingleton<IData, DependencyA>()
            .AddSingleton<IData, DependencyB>()
            .AddSingleton<DependencyC>()
            .BuildServiceProvider();

        this.dependencyA = new DependencyA();
        this.dependencyB = new DependencyB();
        this.dependencyC = new DependencyC();
        this.data = new IData[] { dependencyA, dependencyB };

        this.factoryArguments = new object[] { this.dependencyA, this.dependencyB, this.dependencyC, this.data };
    }

    [Benchmark]
    public void DoserGetService()
    {
        this.doserProvider.GetService<TypeToBeActivated>();
    }

    [Benchmark]
    public void ServiceProviderGetService()
    {
        ServiceProviderServiceExtensions.GetService<TypeToBeActivated>(this.serviceProvider);
    }

    [Benchmark]
    public void ActivatorCreateInstance()
    {
        Activator.CreateInstance(typeof(TypeToBeActivated), factoryArguments);
    }

    [Benchmark]
    public void CreateInstance()
    {
        new TypeToBeActivated(new DependencyA(), new DependencyB(), new DependencyC(), this.data);
    }

    [Benchmark]
    public void CreateInstanceStatic()
    {
        new TypeToBeActivated(this.dependencyA, this.dependencyB, this.dependencyC, this.data);
    }
}

