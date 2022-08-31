namespace Doser.Implementation;

internal interface IObjectResolver
{
    InstanceLifetime Lifetime { get;}

    object? Resolve();

    IObjectResolver Build();
}