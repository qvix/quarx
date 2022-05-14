namespace Doser
{
    using System;

    public interface IScope : IDisposable
    {
        object Get(Guid key, Func<object> next);

        object GetTransparent(Guid key, Func<object> next);
    }
}
