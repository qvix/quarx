namespace Doser.Implementation
{
    internal interface IObjectResolver
    {
        InstanceLifetime Lifetime { get;}

        object Get();

        void Build();
    }
}