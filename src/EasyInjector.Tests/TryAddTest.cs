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
    public class TryAddTest : BaseTest
    {
        [SetUp]
        public void Init()
        {
        }

        [Test]
        public void TryAdd001()
        {
            using (var injector = new EasyInjector())
            {
                injector.AddSingleton<IApi>(sp => new Api1());
                injector.AddSingleton<IApi>(sp => new Api2());
                Assert.True(injector.GetRequiredService<IApi>().GerVersion() == 2);
                injector.AddSingleton<IApi>(sp => new Api1());
                Assert.True(injector.GetRequiredService<IApi>().GerVersion() == 1);
                injector.TryAddSingleton<IApi>(sp => new Api2());
                Assert.True(injector.GetRequiredService<IApi>().GerVersion() == 1);
            }

            using (var injector = new EasyInjector())
            {
                injector.AddTransient<IApi>(sp => new Api1());
                injector.AddTransient<IApi>(sp => new Api2());
                Assert.True(injector.GetRequiredService<IApi>().GerVersion() == 2);
                injector.AddTransient<IApi>(sp => new Api1());
                Assert.True(injector.GetRequiredService<IApi>().GerVersion() == 1);
                injector.TryAddTransient<IApi>(sp => new Api2());
                Assert.True(injector.GetRequiredService<IApi>().GerVersion() == 1);
            }

            using (var injector = new EasyInjector())
            using (var scope = injector.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                injector.AddScoped<IApi>(sp => new Api1());
                injector.AddScoped<IApi>(sp => new Api2());
                Assert.True(scope.ServiceProvider.GetRequiredService<IApi>().GerVersion() == 2);
                injector.AddScoped<IApi>(sp => new Api1());
                Assert.True(scope.ServiceProvider.GetRequiredService<IApi>().GerVersion() == 1);
                injector.TryAddScoped<IApi>(sp => new Api2());
                Assert.True(scope.ServiceProvider.GetRequiredService<IApi>().GerVersion() == 1);
            }
        }

        [Test]
        public void TryAdd002()
        {
            using (var injector = new MyInjector())
            {
                {
                    injector.IsTry = null;
                    injector.TryAddSingleton<IApi>(sp => new Api2());
                    Assert.True(injector.IsTry.Value == true);
                }
                {
                    injector.IsTry = null;
                    injector.TryAddScoped<IApi>(sp => new Api2());
                    Assert.True(injector.IsTry.Value == true);
                }
                {
                    injector.IsTry = null;
                    injector.TryAddTransient<IApi>(sp => new Api2());
                    Assert.True(injector.IsTry.Value == true);
                }
                {
                    injector.IsTry = null;
                    injector.AddSingleton<IApi>(sp => new Api2());
                    Assert.True(injector.IsTry.Value == false);
                }
                {
                    injector.IsTry = null;
                    Assert.Throws<ApplicationException>(() => injector.AddScoped<IApi>(sp => new Api2()));
                }
                {
                    injector.IsTry = null;
                    Assert.Throws<ApplicationException>(() => injector.AddTransient<IApi>(sp => new Api2()));
                }
            }

            using (var injector = new MyInjector())
            {
                var inject1 = new EasyInjector();
                inject1.AddSingleton<IApi>(sp => new Api1());

                injector.IsTry = null;
                injector.TryImportServices(inject1);
                Assert.True(injector.IsTry.Value == true);

                injector.IsTry = null;
                injector.ImportServices(inject1);
                Assert.True(injector.IsTry.Value == false);
            }

            using (var injector = new MyInjector())
            {
                injector.IsTry = null;
                injector.AddGenericService(SimpleLifetimes.Transient, typeof(IProvider<>), (a, b) => new object());
                Assert.True(injector.IsTry.Value == false);

                injector.IsTry = null;
                injector.TryAddGenericService(SimpleLifetimes.Transient, typeof(IProvider<>), (a, b) => new object());
                Assert.True(injector.IsTry.Value == true);
            }
        }



    }

    public class MyInjector : EasyInjector
    {
        public bool? IsTry { get; set; }

        protected internal override void AddServiceInternal(ServiceRegister register, bool isTry = false)
        {
            IsTry = isTry;
            base.AddServiceInternal(register, isTry);
        }

    }

    public interface IApi
    {
        int GerVersion();
    }

    public class Api1 : IApi
    {
        public int GerVersion()
        {
            return 1;
        }
    }

    public class Api2 : IApi
    {
        public int GerVersion()
        {
            return 2;
        }
    }
}
