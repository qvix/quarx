namespace Doser.Implementation;

internal interface IObjectResolver
{
    InstanceLifetime Lifetime { get;}

    object? Resolve();

    void Build();
}