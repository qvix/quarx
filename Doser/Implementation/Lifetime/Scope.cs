namespace Doser.Implementation.Lifetime;

using System.Collections.Generic;
using System.Threading;
using System;

internal class Scope : IScope
{
    private readonly Dictionary<Guid, object> instances = new ();
    private readonly ThreadScopeService service;
    private readonly ReaderWriterLockSlim lockObject = new(LockRecursionPolicy.SupportsRecursion);

    public Scope(ThreadScopeService service, IScope? parent)
    {
        this.service = service;
        this.Parent = parent;
    }

    public IScope? Parent { get; }

    public object? Get(Guid key, Func<object?> factory)
    {
        this.lockObject.EnterUpgradeableReadLock();
        try
        {
            if (this.instances.TryGetValue(key, out var result))
            {
                return result;
            }

            this.lockObject.EnterWriteLock();
            try
            {
                if (this.instances.TryGetValue(key, out result))
                {
                    return result;
                }

                result = factory();
                this.instances.Add(key, result!);
                return result;

            }
            finally
            {
                this.lockObject.ExitWriteLock();
            }
        }
        finally 
        {
            this.lockObject.ExitUpgradeableReadLock();
        }
    }

    public object? GetTransparent(Guid key, Func<object?> factory)
    {
        return this.Get(key, this.Parent == null 
            ? factory 
            : () => this.Parent.GetTransparent(key, factory));
    }

    public void Dispose()
    {
        this.service.CloseScope(this);
    }
}