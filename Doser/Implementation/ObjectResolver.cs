namespace Doser.Implementation
{
    using System;
    using System.Linq;

    internal class ObjectResolver  
    {
        private static readonly Func<object> DefaultResult = () => default;
        private readonly Func<object> getFunction;
        private readonly Action buildAction;

        public ObjectResolver(params IObjectResolver[] resolvers)
        {
            this.getFunction =
                resolvers.Length switch
                {
                    0 => DefaultResult,
                    1 => () => resolvers[0].Resolve(DefaultResult)(),
                    2 => () => resolvers[0].Resolve(resolvers[1].Resolve(DefaultResult))(),
                    _ => BuildGet(resolvers)
                };
            this.buildAction =
                () =>
                {
                    foreach (var resolver in resolvers)
                    {
                        resolver.Build();
                    }
                };
        }

        public object Get() => getFunction();

        public void Build() => buildAction();

        private Func<object> BuildGet(params IObjectResolver[] resolvers)
        {
            var reversed = resolvers.Reverse().ToArray();
            return () =>
            {
                var result = DefaultResult;
                for (var i = 0; i < reversed.Length; i++)
                {
                    result = reversed[i].Resolve(result);
                }

                return result();
            };
        }
    }
}