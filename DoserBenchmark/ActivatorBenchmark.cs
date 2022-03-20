using Microsoft.Extensions.DependencyInjection;

namespace DoserBenchmark;

using BenchmarkDotNet.Attributes;
using Doser;
using DoserBenchmark.Entities;
using System;

public class ActivatorBenchmark
{
    private DoserProvider doserProvider;
    private IServiceProvider serviceProvider;
    private object[] factoryArguments;
    private DependencyA dependencyA;
    private DependencyB dependencyB;
    private DependencyC dependencyC;

    [GlobalSetup]
    public void SetUp()
    {
        this.doserProvider = new DoserProvider();
        this.doserProvider.AddTransient<TypeToBeActivated>();
        this.doserProvider.AddSingleton<DependencyA>();
        this.doserProvider.AddSingleton<DependencyB>();
        this.doserProvider.AddSingleton<DependencyC>();

        var collection = new ServiceCollection();
        collection.AddTransient<TypeToBeActivated>();
        collection.AddSingleton<DependencyA>();
        collection.AddSingleton<DependencyB>();
        collection.AddSingleton<DependencyC>();
        this.serviceProvider = collection.BuildServiceProvider();

        this.dependencyA = new DependencyA();
        this.dependencyB = new DependencyB();
        this.dependencyC = new DependencyC();   

        factoryArguments = new object[] { this.dependencyA, this.dependencyB, this.dependencyC };
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
        new TypeToBeActivated(new DependencyA(), new DependencyB(), new DependencyC());
    }

    [Benchmark]
    public void CreateInstanceStatic()
    {
        new TypeToBeActivated(this.dependencyA,this.dependencyB, this.dependencyC);
    }
}

