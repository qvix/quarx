namespace DoserBenchmark
{
    using BenchmarkDotNet.Running;
    using IQbx.DoserBenchmark;

    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(ActivatorBenchmark));
            //BenchmarkRunner.Run(typeof(ScopeValidationBenchmark));
        }
    }
}
