namespace Doser.Implementation
{
    using System;

    internal interface IObjectResolver
    {
        Func<object> Resolve(Func<object> next);

        void Build()
        {
        }
    }
}