namespace IQbx.Doser
{
    using System;
    using System.Runtime.CompilerServices;

    public static class ArgumentExtensions
    {
        public static void CheckNotNull<T>(this T value, [CallerArgumentExpression("name")] string name = "")
        {
            if (value == null)
            {
                throw new ArgumentException(name);
            }
        }
    }
}