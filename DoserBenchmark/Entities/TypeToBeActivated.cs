namespace DoserBenchmark.Entities;

using System.Collections.Generic;

public class TypeToBeActivated
{
    public TypeToBeActivated(DependencyA a, DependencyB b, DependencyC c, IEnumerable<IData> data)
    {
        this.Data = data;
    }

    public IEnumerable<IData> Data { get; }
}