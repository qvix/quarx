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
        return this.value ??= objectResolver.Resolve();
    }

    public void Build()
    {
        this.objectResolver.Build();
    }
}