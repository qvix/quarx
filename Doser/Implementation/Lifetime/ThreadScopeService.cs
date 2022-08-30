namespace Doser.Implementation.Lifetime;

using System.Threading;

public class ThreadScopeService : IScopeService
{
    private readonly AsyncLocal<IScope?> current = new ();

    public IScope? Current { get => current.Value; private set => current.Value = value; }

    public IScope CreateScope()
    {
        return this.Current = new Scope(this, this.Current);
    }

    internal void CloseScope(Scope scope)
    {
        this.Current = scope.Parent;
    }
}