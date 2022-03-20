namespace DoserTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Doser;
    using Doser.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DoserTests
    {
        [TestMethod]
        public void SingletonInjectedTypeShouldResolve()
        {
            var doser = new DoserProvider();

            doser.AddSingleton<ITest, Test>();

            var first = doser.GetService<ITest>();
            Assert.IsNotNull(first);

            var second = doser.GetService<ITest>();
            Assert.IsNotNull(second);

            Assert.AreEqual(first.Value, second.Value);
        }

        [TestMethod]
        public void TransientInjectedTypeShouldResolve()
        {
            var doser = new DoserProvider();

            doser.AddTransient<ITest, Test>();

            var first = doser.GetService<ITest>();
            Assert.IsNotNull(first);

            var second = doser.GetService<ITest>();
            Assert.IsNotNull(second);

            Assert.AreNotEqual(first.Value, second.Value);
        }

        [TestMethod]
        public void ComplexInjectedTypeShouldResolve()
        {
            var container = new DoserProvider();

            container
                .AddSingleton<ITest, Test>()
                .AddSingleton<IInjected, Injected>();

            Assert.IsNotNull(container.GetService<IInjected>().Value);
            Assert.IsNotNull(container.GetService<IInjected>().Value);
        }

        [TestMethod]
        public void ComplexInjectedTypeWithKeyShouldResolve()
        {
            var container = new DoserProvider();

            container
                .AddSingleton<ITest, Test>("two")
                .AddSingleton<ITest, Test2>("one")
                .AddSingleton<IInjected, Injected2>();

            var result = container.GetService<IInjected>();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ResolveShouldFailOnInterfaceCreation()
        {
            var container = new DoserProvider();
            container.GetService<IInjected>();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ResolveShouldFailOnCreateObjectWithInvalidKey()
        {
            var container = new DoserProvider()
                .AddSingleton<ITest, Test>();

            container.GetService<Injected2>();
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ResolveShouldFailOnAbstractClassCreation()
        {
            var container = new DoserProvider();
            container.GetService<Base>();
        }

        [TestMethod]
        public void ResolveShouldResolveClass()
        {
            var container = new DoserProvider();
            container.GetService<Test>();
        }
        
        [TestMethod]
        public void ContainerShouldResolveRegisteredInstances()
        {
            var container = new DoserProvider();

            container.AddSingleton(new Test());

            Assert.IsNotNull(container.GetService<Test>());
        }

        [TestMethod]
        public void ContainerShouldResolveRegisteredTypedInstances()
        {
            var container = new DoserProvider();

            container.AddSingleton<ITest>(new Test());

            Assert.IsNotNull(container.GetService<ITest>());
        }

        [TestMethod]
        public void ContainerShouldResolveCustomCreatedInstance()
        {
            var container = new DoserProvider();

            container.AddSingleton<ITest>(() => new Test());

            var first = container.GetService<ITest>();
            Assert.IsNotNull(first);
        }

        [TestMethod]
        public void ContainerShouldResolveEnumerable()
        {
            var container = new DoserProvider();

            container
                .AddSingleton<ITest, Test>()
                .AddSingleton<ITest, Test2>("duo");

            var result = container.GetService<IEnumerable<ITest>>();

            Assert.IsNotNull(result);
            var list = result.ToList();
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(typeof(Test), list.First().GetType());
            Assert.AreEqual(typeof(Test2), list.Skip(1).First().GetType());

            var resultArray = container.GetService<ITest[]>();

            Assert.IsNotNull(resultArray);
            Assert.AreEqual(2, resultArray.Count());
            Assert.AreEqual(typeof(Test), resultArray.First().GetType());
            Assert.AreEqual(typeof(Test2), resultArray.Skip(1).First().GetType());

            var resultList = container.GetService<IList<ITest>>();

            Assert.IsNotNull(resultList);
            Assert.AreEqual(2, resultList.Count());
            Assert.AreEqual(typeof(Test), resultList.First().GetType());
            Assert.AreEqual(typeof(Test2), resultList.Skip(1).First().GetType());
        }

        [TestMethod]
        public void ContainerShouldResolveFunc()
        {
            var container = new DoserProvider();

            container
                .AddSingleton<ITest, Test>()
                .AddSingleton<Test3>();

            var expected = container.GetService<ITest>();
            var result = container.GetService<Test3>();
            Assert.AreEqual(expected.Value, result.Value().Value);
        }

        [TestMethod]
        public void ContainerShouldResolveLazy()
        {
            var container = new DoserProvider();

            container
                .AddSingleton<ITest, Test>()
                .AddSingleton<Test4>();

            var expected = container.GetService<ITest>();
            var result = container.GetService<Test4>();
            Assert.AreEqual(expected.Value, result.Value.Value.Value);
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
            public int Value { get { return 2; } }
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

        #endregion
    }
}
