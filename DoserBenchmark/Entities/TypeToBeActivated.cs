namespace DoserBenchmark.Entities;

using System.Collections.Generic;

public class TypeToBeActivated
{
    public TypeToBeActivated(DependencyA a, DependencyB b, DependencyC c, IEnumerable<IData> data)
    {
        this.A = a;
        this.B = b;
        this.C = c;
        this.Data = data;
    }

    public IEnumerable<IData> Data { get; }

    public DependencyA A { get; }

    public DependencyB B { get; }

    public DependencyC C { get; }
}