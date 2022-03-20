namespace Doser
{
    using System;

    internal interface IDoserServiceProvider : IServiceProvider
    {
        object? GetService(Type serviceType, object key);
    }
}
