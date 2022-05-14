namespace Doser.Exceptions
{
    using System;

    public class ResolveException : Exception
    {
        public ResolveException() { }

        public ResolveException(Type type) : this($"Unable to resolve type {type.FullName}"){ }

        public ResolveException(Type type, object key) : this($"Unable to resolve type {type.FullName} with ket {key}") { }

        public ResolveException(string message) : base(message) { }

        public ResolveException(string message, Exception innerException) : base(message, innerException) { }
    }
}