namespace DoserTests
{
    using Doser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class DoserScopeTests
    {
        [TestMethod]
        public void ScopedShouldResolveInsideScope()
        {
            var doser = new DoserProvider()
                .AddScoped<ITest, Test>()
                .Build();

            using (doser.CreateScope())
            {
                var first = doser.GetService<ITest>();
                Assert.IsNotNull(first);
            }

            var second = doser.GetService<ITest>();
            Assert.IsNull(second);
        }

        [TestMethod]
        public void ScopedShouldCreateNewObjectInsideScope()
        {
            var doser = new DoserProvider()
                .AddScoped<ITest, Test>()
                .Build();

            using (doser.CreateScope())
            {
                var first = doser.GetService<ITest>();
                Assert.IsNotNull(first);

                using (doser.GetService<IScopeService>()!.CreateScope())
                {
                    var second = doser.GetService<ITest>();
                    Assert.IsNotNull(second);

                    Assert.AreNotEqual(first, second);
                }

                var check = doser.GetService<ITest>();
                Assert.IsNotNull(check);

                Assert.AreEqual(first, check);
            }

        }

        [TestMethod]
        public void TransparentScopedShouldResolveOuterScope()
        {
            var doser = new DoserProvider()
                .AddScopeTransparent<ITest, Test>()
                .Build();

            using (doser.CreateScope())
            {
                var first = doser.GetService<ITest>();
                Assert.IsNotNull(first);
                using (doser.CreateScope())
                {
                    var second = doser.GetService<ITest>();
                    Assert.IsNotNull(second);

                    Assert.AreEqual(first, second);
                }
            }
        }

        #region test classes

        private interface ITest
        {
            int Value { get; }
        }

        private interface IInjected
        {
            int Value { get; }
        }

        private abstract class Base
        {
        }

        private class Test : Base, ITest
        {
            public Test()
            {
                this.Value = Guid.NewGuid().GetHashCode();
            }

            public int Value {  get; }
        }

        private class Test2 : ITest
        {
            public int Value => 2;
        }

        private class Test3
        {
            public Test3(Func<ITest> value)
            {
                this.Value = value;
            }

            public Func<ITest> Value { get; }
        }

        private class Test4
        {
            public Test4(Lazy<ITest> value)
            {
                this.Value = value;
            }

            public Lazy<ITest> Value { get; }
        }

        private class Injected : IInjected
        {
            private readonly ITest test;

            public Injected(ITest test)
            {
                this.test = test;
            }

            public int Value => this.test.Value;
        }

        private class Injected2 : IInjected
        {
            private readonly ITest test;

            public Injected2([Dependency("one")] ITest test)
            {
                this.test = test;
            }

            public int Value => this.test.Value;
        }

        private class Injected3
        {
            public Injected3(ITest[] values)
            {
                this.Values = values;
            }

            public ITest[] Values { get; }
        }

        private class Injected4
        {
            public Injected4(IList<ITest> values)
            {
                this.Values = values;
            }

            public IList<ITest> Values { get; }
        }
        #endregion
    }
}
