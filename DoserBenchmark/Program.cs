namespace DoserBenchmark;

using BenchmarkDotNet.Running;

internal class Program
{
    static void Main(string[] args)
    {
        //Test();
        BenchmarkRunner.Run(typeof(ActivatorBenchmark));
        //BenchmarkRunner.Run(typeof(ScopeValidationBenchmark));
    }

    static void Test()
    {
        var benchmark = new ActivatorBenchmark();

        benchmark.SetUp();
        benchmark.DoserGetService();
        benchmark.DoserGetService();
    }
}

