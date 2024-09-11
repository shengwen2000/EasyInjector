using EasyInjectors;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [Category("Common")]
    [TestFixture]
    public class GenericTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Generic001()
        {
            // IsConstructedGenericType 有參數的泛型
            var type2 = typeof(IProvider<Object>);
            Assert.That(type2.IsConstructedGenericType);

            var type1 = typeof(IProvider<>);
            Assert.That(!type1.IsConstructedGenericType);
        }

        [Test]
        public void Generic002()
        {
            // IsConstructedGenericType 有參數的泛型
            var type0 = typeof(IProvider<>);
            var type1 = typeof(IProvider<Object>);
            var type2 = typeof(IProvider<string>);

            Assert.False(type0 == type1);
            Assert.False(type1 == type2);

            Assert.True(type1.GetGenericTypeDefinition() == type2.GetGenericTypeDefinition());
            Assert.True(type0 == type2.GetGenericTypeDefinition());
        }       
    }
}
