using System.Reflection;
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
    private Func<Offspring> createFunctionExpression;
    private Func<object> createFunctionIl;

    [GlobalSetup]
    public void SetUp()
    {
        this.doserProvider = new DoserProvider()
            .AddTransient<Offspring>()
            .AddSingleton<DependencyA>()
            .AddSingleton<DependencyB>()
            .AddSingleton<IData, DependencyA>()
            .AddSingleton<IData, DependencyB>()
            .AddSingleton<DependencyC>()
            .Build();

        this.serviceProvider = new ServiceCollection()
            .AddTransient<Offspring>()
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
    }


    [Benchmark]
    public void DoserGetService()
    {
        var offspring = this.doserProvider.GetService<Offspring>();
        offspring.Foo();
    }

    [Benchmark]
    public void ServiceProviderGetService()
    {
        var offspring = this.serviceProvider.GetService<Offspring>();
        offspring.Foo();
    }

    [Benchmark]
    public void ExpressionCreate()
    {
        var offspring = this.createFunctionExpression();
        offspring.Foo();
    }

    [Benchmark]
    public void IlCreate()
    {
        //this.createFunctionIl();
    }
    
    [Benchmark]
    public void ActivatorCreateInstance()
    {
        var offspring = (Offspring)Activator.CreateInstance(typeof(Offspring), factoryArguments);
        offspring.Foo();
    }

    [Benchmark]
    public void CreateInstance()
    {
        var a = new DependencyA();
        var b = new DependencyB();
        var c = new DependencyC();
        var d = new IData[] { a, b };

        var offspring = new Offspring(a, b, c, d);
        offspring.Foo();
    }

    [Benchmark]
    public void CreateInstanceStatic()
    {
        var offspring = new Offspring(this.dependencyA, this.dependencyB, this.dependencyC, this.data);
        offspring.Foo();
    }
    
    private Func<Offspring> GetExpressionCreationFunction()
    {
        var constructor = typeof(Offspring).GetConstructors()[0];

        var parameters = new Expression[]
        {
            Expression.Constant(this.dependencyA),
            Expression.Constant(this.dependencyB),
            Expression.Constant(this.dependencyC), 
            Expression.Constant(this.data)
        };

        return (Func<Offspring>)Expression.Lambda(Expression.New(constructor, parameters)).Compile();
    }
    

    private Func<object> GetIlCreationFunction()
    {
        var method = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(object), Type.EmptyTypes, this.GetType(), true);
        var generator = method.GetILGenerator();

        var constructor = typeof(Offspring).GetConstructors()[0];
        
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldfld, typeof(ActivatorBenchmark).GetField(nameof(dependencyA), BindingFlags.NonPublic | BindingFlags.Instance)!);

        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldfld, typeof(ActivatorBenchmark).GetField(nameof(dependencyB), BindingFlags.NonPublic | BindingFlags.Instance)!);

        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldfld, typeof(ActivatorBenchmark).GetField(nameof(dependencyC), BindingFlags.NonPublic | BindingFlags.Instance)!);

        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldfld, typeof(ActivatorBenchmark).GetField(nameof(data), BindingFlags.NonPublic | BindingFlags.Instance)!);
        
        generator.Emit(OpCodes.Newobj, constructor);
        generator.Emit(OpCodes.Ret);

        return (Func<object>)method.CreateDelegate(typeof(Func<object>));
    }
}

