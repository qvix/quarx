namespace DoserBenchmark.Entities;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Offspring
{
    public Offspring(DependencyA a, DependencyB b, DependencyC c, IEnumerable<IData> data)
    {
        this.A = a;
        this.B = b;
        this.C = c;
        this.Data = data;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Foo()
    {

    }

    public IEnumerable<IData> Data { get; }

    public DependencyA A { get; }

    public DependencyB B { get; }

    public DependencyC C { get; }
}