namespace Doser;

public interface IScopeService
{
    IScope Current { get; }

    IScope CreateScope();
}