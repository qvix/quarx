namespace IQbx.Doser.Implementation
{
    using System;
    using System.Linq;

    internal static class TypeExtensions
    {
        public static bool IsInterfaceImplementor(this Type value, Type interfaceType)
        {
            return value == interfaceType
                   || (value.IsGenericType && value.GetGenericTypeDefinition() == interfaceType)
                   || value.GetInterfaces().Contains(interfaceType)
                   || IsInheritedFromGenericInterface(value, interfaceType);
        }

        public static bool IsInheritedFrom(this Type type, Type ancestor)
        {
            return type == ancestor || type.IsSubclassOf(type) || IsInheritedFromGeneric(type, ancestor);
        }


        private static bool IsInheritedFromGenericInterface(Type type, Type ancestor)
        {
            var interfaces = type.GetInterfaces();
            return interfaces
                .Select(typeInterface => typeInterface.IsGenericType ? typeInterface.GetGenericTypeDefinition() : typeInterface)
                .Any(current => current == ancestor);
        }

        private static bool IsInheritedFromGeneric(Type type, Type ancestor)
        {
            while (type != null && type != typeof(object))
            {
                var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (current == ancestor)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}