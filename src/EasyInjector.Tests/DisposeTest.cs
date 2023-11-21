using EasyInjectors;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace Tests
{
    [Category("EasyInjector")]
    [TestFixture]
    public class DisposeTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Dispose001()
        {
            using (var injector = new EasyInjector())
            {
                // 包裝自身的服務
                injector.AddSingleton<IMyServiceProvider>(sp => new MyServiceProvider(injector));

                var srv1 = injector.GetRequiredService<IMyServiceProvider>();

                Assert.NotNull(srv1);
            }     
            // 沒有異常表示通過 dispose驗證
        }

        public interface IMyServiceProvider : IServiceProvider, IDisposable
        {
        }

        class MyServiceProvider : IMyServiceProvider
        {
            private EasyInjector _provider;
            public MyServiceProvider(EasyInjector provider)
            {
                _provider = provider;
            }

            public object GetService(Type serviceType)
            {
                return _provider.GetService(serviceType);
            }

            // dispose 可能形成無限迴圈
            public void Dispose()
            {
                _provider.Dispose();
            }
        }       
    }
}
