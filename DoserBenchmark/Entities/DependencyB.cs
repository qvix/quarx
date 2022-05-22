using System.Diagnostics;

namespace DoserBenchmark.Entities;

public class DependencyB : IData
{
    public DependencyB()
    {
        Debug.WriteLine("Create DependencyB");
    }

    public int Value() => 5;
}