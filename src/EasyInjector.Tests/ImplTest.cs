using EasyInjectors;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    /// <summary>
    /// 實作類別測試
    /// </summary>
    [Category("EasyInjector")]
    [TestFixture]
    public class ImplTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
            //每個測試方法執行前都會執行的動作
        }

        [Test]
        public void Impl001()
        {
            using (var injector = new EasyInjector())
            {
                injector.AddSingleton<IApi1, Api1>();
                injector.AddSingleton<IApi2, Api2>();

                var api1 = injector.GetRequiredService<IApi1>();
                Assert.NotNull(api1);
                var api2 = injector.GetRequiredService<IApi2>() as Api2;
                Assert.NotNull(api2);
                Assert.NotNull(api2.Api1);
            }
        }

        [Test]
        public void Impl002()
        {
            using (var injector = new EasyInjector())
            {
                injector.AddSingleton<IApi1, Api1>();
                injector.AddSingleton<Api2, Api2>();

                var api2 = injector.GetRequiredService<Api2>();
                Assert.NotNull(api2);
                Assert.NotNull(api2.Api1);
            }
        }

        [Test]
        public void Impl003()
        {
            using (var injector = new EasyInjector())
            {
                injector.AddScoped<IApi1, Api1>();
                injector.AddScoped<IApi2, Api2>();

                using (var scope = injector.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var api1 = scope.ServiceProvider.GetRequiredService<IApi1>();
                    Assert.NotNull(api1);
                    var api2 = scope.ServiceProvider.GetRequiredService<IApi2>() as Api2;
                    Assert.NotNull(api2);
                    Assert.NotNull(api2.Api1);
                }
            }
        }

        [Test]
        public void Impl004()
        {
            using (var injector = new EasyInjector())
            {
                injector.AddSingleton(typeof(IApi1), typeof(Api1));
                injector.AddSingleton(typeof(IApi2), typeof(Api2));

                var api1 = injector.GetRequiredService<IApi1>();
                Assert.NotNull(api1);
                var api2 = injector.GetRequiredService<IApi2>() as Api2;
                Assert.NotNull(api2);
                Assert.NotNull(api2.Api1);
            }

            using (var injector = new EasyInjector())
            {
                injector.AddTransient(typeof(IApi1), typeof(Api1));
                injector.AddTransient(typeof(IApi2), typeof(Api2));

                var api1 = injector.GetRequiredService<IApi1>();
                Assert.NotNull(api1);
                var api1_1 = injector.GetRequiredService<IApi1>();
                Assert.NotNull(api1_1);
                Assert.True(api1 != api1_1);

                var api2 = injector.GetRequiredService<IApi2>();
                Assert.NotNull(api2);
            }

            using (var injector = new EasyInjector())
            {
                injector.AddScoped(typeof(IApi1), typeof(Api1));
                injector.AddScoped(typeof(IApi2), typeof(Api2));

                using (var scope = injector.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var api1 = scope.ServiceProvider.GetRequiredService<IApi1>();
                    Assert.NotNull(api1);
                    var api2 = scope.ServiceProvider.GetRequiredService<IApi2>() as Api2;
                    Assert.NotNull(api2);
                    Assert.NotNull(api2.Api1);
                }
            }
        }

        public interface IApi1
        {

        }

        public interface IApi2
        {

        }        

        class Api1 : IApi1
        {
            public Api1()
            {

            }
        }

        class Api2 : IApi2
        {
            public Api2(IApi1 api1)
            {
                Api1 = api1;
            }

            public IApi1 Api1 { get; set; }
        }        
    }
}
