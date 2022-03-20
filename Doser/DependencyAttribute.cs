namespace Doser
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class DependencyAttribute: Attribute
    {
        public DependencyAttribute(string key = null)
        {
            this.Key = key;
        }

        public string Key { get; }
    }
}