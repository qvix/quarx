namespace Doser
{
    using System;

    public interface IDoserServiceProvider : IServiceProvider
    {
        object? GetService(Type serviceType, object key);
    }
}
