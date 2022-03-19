namespace IQbx.Doser.Implementation
{
    using System;

    public interface IScopeService
    {
        object Get(Guid key, Func<object> next);
        
        void Close();
    }
}