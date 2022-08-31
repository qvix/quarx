namespace Doser.Implementation.Lifetime;

internal class SingletonLifetime : IObjectResolver
{
    private readonly IObjectResolver objectResolver;
    private object? value;

    public SingletonLifetime(IObjectResolver objectResolver)
    {
        this.objectResolver = objectResolver;
    }

    public InstanceLifetime Lifetime => InstanceLifetime.Global;

    public object? Resolve()
    {
        return this.value ??= this.objectResolver.Resolve();
    }

    public IObjectResolver Build()
    {
        this.objectResolver.Build();
        this.value ??= this.objectResolver.Resolve();
        return this;
    }
}