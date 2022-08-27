using System;

namespace Doser.Implementation;

internal interface IObjectResolver
{
    InstanceLifetime Lifetime { get;}

    Func<object> GetResolver();

    void Build();
}