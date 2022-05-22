using System.Collections;

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
            var doser = new DoserProvider()
                .AddSingleton<ITest, Test>()
                .Build();

            var first = doser.GetService<ITest>();
            Assert.IsNotNull(first);

            var second = doser.GetService<ITest>();
            Assert.IsNotNull(second);

            Assert.AreEqual(first.Value, second.Value);
        }

        [TestMethod]
        public void TransientInjectedTypeShouldResolve()
        {
            var doser = new DoserProvider()
                .AddTransient<ITest, Test>()
                .Build();

            var first = doser.GetService<ITest>();
            Assert.IsNotNull(first);

            var second = doser.GetService<ITest>();
            Assert.IsNotNull(second);

            Assert.AreNotEqual(first.Value, second.Value);
        }

        [TestMethod]
        public void ComplexInjectedTypeShouldResolve()
        {
            var provider =  new DoserProvider()
                .AddSingleton<ITest, Test>()
                .AddSingleton<IInjected, Injected>()
                .Build();

            Assert.IsNotNull(provider.GetService<IInjected>().Value);
            Assert.IsNotNull(provider.GetService<IInjected>().Value);
        }

        [TestMethod]
        public void ComplexInjectedTypeWithKeyShouldResolve()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest, Test>("two")
                .AddSingleton<ITest, Test2>("one")
                .AddSingleton<IInjected, Injected2>()
                .Build();

            var result = provider.GetService<IInjected>();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ResolveShouldFailOnInterfaceCreation()
        {
            new DoserProvider()
                .Build()
                .GetService<IInjected>();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ResolveShouldFailOnCreateObjectWithInvalidKey()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest, Test>()
                .Build();

            provider.GetService<Injected2>();
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ResolveShouldFailOnAbstractClassCreation()
        {
            var provider = new DoserProvider().Build();
            provider.GetService<Base>();
        }

        [TestMethod]
        public void ResolveShouldResolveClass()
        {
            var provider = new DoserProvider().Build();
            provider.GetService<Test>();
        }
        
        [TestMethod]
        public void ProviderShouldResolveRegisteredInstances()
        {
            var provider = new DoserProvider()
                .AddSingleton(new Test())
                .Build();

            Assert.IsNotNull(provider.GetService<Test>());
        }

        [TestMethod]
        public void ProviderShouldResolveRegisteredTypedInstances()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest>(new Test())
                .Build();

            Assert.IsNotNull(provider.GetService<ITest>());
        }

        [TestMethod]
        public void ProviderShouldResolveCustomCreatedInstance()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest>(() => new Test())
                .Build();

            var first = provider.GetService<ITest>();
            Assert.IsNotNull(first);
        }

        [TestMethod]
        public void ProviderShouldResolveEnumerable()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest, Test>()
                .AddSingleton<ITest, Test2>("duo")
                .Build();

            var result = provider.GetService<IEnumerable<ITest>>();

            Assert.IsNotNull(result);
            var list = result.ToList();
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(typeof(Test), list.First().GetType());
            Assert.AreEqual(typeof(Test2), list.Skip(1).First().GetType());

            var resultArray = provider.GetService<ITest[]>();

            Assert.IsNotNull(resultArray);
            Assert.AreEqual(2, resultArray.Count());
            Assert.AreEqual(typeof(Test), resultArray.First().GetType());
            Assert.AreEqual(typeof(Test2), resultArray.Skip(1).First().GetType());

            var resultList = provider.GetService<IList<ITest>>();

            Assert.IsNotNull(resultList);
            Assert.AreEqual(2, resultList.Count());
            Assert.AreEqual(typeof(Test), resultList.First().GetType());
            Assert.AreEqual(typeof(Test2), resultList.Skip(1).First().GetType());
        }

        [TestMethod]
        public void ProviderShouldResolveFunc()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest, Test>()
                .AddSingleton<Test3>()
                .Build();

            var expected = provider.GetService<ITest>();
            var result = provider.GetService<Test3>();
            Assert.AreEqual(expected.Value, result.Value().Value);
        }

        [TestMethod]
        public void ProviderShouldResolveLazy()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest, Test>()
                .AddSingleton<Test4>()
                .Build();

            var expected = provider.GetService<ITest>();
            var result = provider.GetService<Test4>();
            Assert.AreEqual(expected.Value, result.Value.Value.Value);
        }

        [TestMethod]
        public void ProviderShouldResolveArray()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest, Test>()
                .AddSingleton<ITest, Test2>()
                .Build();

            var result = provider.GetService<Injected3>();
            Assert.AreEqual(2, result.Values.Length);
        }

        [TestMethod]
        public void ProviderShouldResolveList()
        {
            var provider = new DoserProvider()
                .AddSingleton<ITest, Test>()
                .AddSingleton<ITest, Test2>()
                .Build();

            var result = provider.GetService<Injected4>();
            Assert.AreEqual(2, result.Values.Count);
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
