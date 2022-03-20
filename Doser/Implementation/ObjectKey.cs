namespace Doser.Implementation
{
    using System;

    internal sealed record ObjectKey : IEquatable<ObjectKey>
    {
        public Type Type { get; init; }

        public object? Key { get; init; }

        public bool Equals(ObjectKey? other)
        {
            return ReferenceEquals(this, other)
                || this.Type == other.Type && ((this.Key == null && other.Key == null) || this.Key.Equals(other.Key));
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode() ^ (this.Key?.GetHashCode() ?? 0);
        }
    }
}
