using System.Diagnostics;

namespace DoserBenchmark.Entities;

public class DependencyA : IData
{
    public DependencyA()
    {
        Debug.WriteLine("Create DependencyA");
    }

    public int Value() => 3;
}