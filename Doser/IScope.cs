namespace Doser;

using System;

public interface IScope : IDisposable
{
    object Get(Guid key, Func<object> factory);

    object GetTransparent(Guid key, Func<object> factory);
}