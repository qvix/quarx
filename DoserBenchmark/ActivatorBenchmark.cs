using Microsoft.Extensions.DependencyInjection;

namespace IQbx.DoserBenchmark;

using BenchmarkDotNet.Attributes;
using IQbx.Doser;
using IQbx.DoserBenchmark.Entities;
using System;

public class ActivatorBenchmark
{
    private Doser doser;
    private IServiceProvider serviceProvider;
    private object[] factoryArguments;
    private DependencyA dependencyA;
    private DependencyB dependencyB;
    private DependencyC dependencyC;

    [GlobalSetup]
    public void SetUp()
    {
        this.doser = new Doser();
        this.doser.AddTransient<TypeToBeActivated>();
        this.doser.AddSingleton<DependencyA>();
        this.doser.AddSingleton<DependencyB>();
        this.doser.AddSingleton<DependencyC>();

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
        this.doser.GetService<TypeToBeActivated>();
    }

    [Benchmark]
    public void ServiceProviderGetService()
    {
        this.serviceProvider.GetService<TypeToBeActivated>();
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

