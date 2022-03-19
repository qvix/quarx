namespace IQbx.Doser.Implementation
{
    using System;

    internal interface IObjectResolver
    {
        Func<object> Resolve(Func<object> next);
    }
}