using Microsoft.Extensions.DependencyInjection;

namespace DoserBenchmark;

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;

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
    private Func<object> createFunctionExpression;
    private Func<object> createFunctionIl;
    private Func<object> createFunctionDoser;

    [GlobalSetup]
    public void SetUp()
    {
        this.doserProvider = new DoserProvider()
            .AddTransient<Offstring>()
            .AddSingleton<DependencyA>()
            .AddSingleton<DependencyB>()
            .AddSingleton<IData, DependencyA>()
            .AddSingleton<IData, DependencyB>()
            .AddSingleton<DependencyC>()
            .Build();

        this.serviceProvider = new ServiceCollection()
            .AddTransient<Offstring>()
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

        this.createFunctionExpression = this.GetExpressionCreationFunction();
        this.createFunctionIl = this.GetIlCreationFunction();
        this.createFunctionDoser = this.doserProvider.GetResolver<Offstring>();
    }

    //[Benchmark]
    public void  DoserGetServiceDirect()
    {
        var offspring = (Offstring)this.createFunctionDoser();
        offspring.Foo();
    }

    [Benchmark]
    public void DoserGetService()
    {
        var offspring = this.doserProvider.GetService<Offstring>();
        offspring.Foo();
    }

    //[Benchmark]
    public void DoserGetResolver()
    {
        this.doserProvider.GetResolver<Offstring>();
    }

    [Benchmark]
    public void ServiceProviderGetService()
    {
        var offspring = this.serviceProvider.GetService<Offstring>();
        offspring.Foo();
    }

    [Benchmark]
    public void ExpressionCreate()
    {
        this.createFunctionExpression();
    }

    [Benchmark]
    public void IlCreate()
    {
        this.createFunctionIl();
    }

    [Benchmark]
    public void ActivatorCreateInstance()
    {
        var offspring = (Offstring)Activator.CreateInstance(typeof(Offstring), factoryArguments);
        offspring.Foo();
    }

    [Benchmark]
    public void CreateInstance()
    {
        var a = new DependencyA();
        var b = new DependencyB();
        var c = new DependencyC();
        var d = new IData[] { a, b };

        var offspring = new Offstring(a, b, c, d);
        offspring.Foo();
    }

    [Benchmark]
    public void CreateInstanceStatic()
    {
        new Offstring(this.dependencyA, this.dependencyB, this.dependencyC, this.data);
    }

    private Func<object> GetExpressionCreationFunction()
    {
        var constructor = typeof(DependencyA).GetConstructors()[0];
        
        return (Func<object>)Expression.Lambda(Expression.New(constructor)).Compile();
    }

    private Func<object> GetIlCreationFunction()
    {
        var method = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(object), Type.EmptyTypes, this.GetType(), true);
        var generator = method.GetILGenerator();

        var constructor = typeof(DependencyA).GetConstructors()[0];
        
        generator.Emit(OpCodes.Newobj, constructor);
        generator.Emit(OpCodes.Ret);

        return (Func<object>)method.CreateDelegate(typeof(Func<object>));
    }
}

