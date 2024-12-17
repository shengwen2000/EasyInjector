using Microsoft.Extensions.DependencyInjection;
using EasyInjectors.Dev;
using Tests.Overrides;
using System.Reflection;
using System.Collections;

namespace Tests
{
    [TestFixture]
    public class OverrideTest : BaseTest
    {
        [Test, Apartment(ApartmentState.STA)]
        public async Task Override001()
        {
            var services = new ServiceCollection();

            services.AddScoped<ILoginService, LoginService>();
            // 複寫測試類別
            services.AddOverride<ILoginService, LoginServiceDev>();

            var provider = services.BuildServiceProvider(true);

            var scope = provider.CreateScope();
            var api = scope.ServiceProvider.GetRequiredService<ILoginService>();
            var ret = api.Login(new Login { Account = "david", Password = "{error password}" });
            Assert.That(ret, Is.EqualTo("OK"));

            var ret2 = await api.Login2(new Login { Account = "david", Password = "123456" });
            Assert.That(ret2, Is.EqualTo("OK"));

            var ret3 = api.Hello();
            Assert.That(ret3, Is.EqualTo("World World Dev"));

            var ret4 = api.GetName();
            Assert.That(ret4, Is.EqualTo("David"));
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Override002()
        {
            var services = new ServiceCollection();

            services.AddScoped<ILoginService, LoginService>();
            // 複寫測試類別
            services.AddOverride<ILoginService, LoginServiceDev>();
            // 複寫測試類別
            services.AddOverride<ILoginService, LoginServiceDev>();

            var provider = services.BuildServiceProvider(true);

            var scope = provider.CreateScope();
            var api = scope.ServiceProvider.GetRequiredService<ILoginService>();

            var ret3 = api.Hello();
            Assert.That(ret3, Is.EqualTo("World World Dev World Dev"));
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Override002A()
        {
            var services = new ServiceCollection();
            services.AddScoped<ILoginService, LoginService>();
            var provider = services.BuildServiceProvider(true);

            var scope = provider.CreateScope();

            var api = scope.ServiceProvider.CreateOverrideInstance<ILoginService, LoginServiceDev>();

            var ret3 = api.Hello();
            Assert.That(ret3, Is.EqualTo("World World Dev"));
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Override002B()
        {
            var services = new ServiceCollection();
            services.AddScoped<ILoginService, LoginService>();
            var provider = services.BuildServiceProvider(true);

            var scope = provider.CreateScope();
            var srv1 = scope.ServiceProvider.GetRequiredService<ILoginService>();
            var srv2 = scope.ServiceProvider.CreateOverrideInstance<ILoginService, LoginServiceDev>(srv1);
            Assert.That(srv2.Account, Is.EqualTo("david"));
            var ret2 = srv2.Hello();
            Assert.That(ret2, Is.EqualTo("World World Dev"));

            var ret1 = srv1.Hello();
            Assert.That(ret1, Is.EqualTo("World"));
            Assert.That(srv1.Account, Is.EqualTo("david"));
        }



        //[Test, Apartment(ApartmentState.STA)]
        public void Override003()
        {
            {
                var services = new ServiceCollection();
                services.AddSingleton(typeof(IMyLogger<>), typeof(MyLoggerImpl<>));
                services.AddSingleton(typeof(IMyLogger<>), typeof(MyLoggerImpl2<>));
                services.BuildServiceProvider(true);
            }

            {
                var services = new ServiceCollection();
                services.AddSingleton(typeof(IMyLogger<>), typeof(MyLoggerImpl<>));
                services.AddOverride<IMyLogger<string>, MyLoggerImpl2<string>>();
                var provider = services.BuildServiceProvider(true);

                var srv = provider.GetRequiredService<IMyLogger<string>>();
                srv.LogInfo("hello");
            }
        }

        /// <summary>
        /// injected Key Service
        /// </summary>
        [Test, Apartment(ApartmentState.STA)]
        public void Override004()
        {
            var services = new ServiceCollection();
            var demoQueue = new Queue();
            demoQueue.Enqueue("Demo");
            var devQueue = new Queue();
            devQueue.Enqueue("Dev");

            services.AddKeyedSingleton<Queue>("Demo", (sp, name) => demoQueue);
            services.AddKeyedSingleton<Queue>("Dev", (sp, name) => devQueue);
            services.AddScoped<IDemoService, DemoService>();
            services.AddOverride<IDemoService, DemoServiceDev>();

            var provider = services.BuildServiceProvider(true);

            var scope = provider.CreateScope();
            var srv1 = scope.ServiceProvider.GetRequiredService<IDemoService>();
            var txt = srv1.SayHello();
            Assert.That(txt, Is.EqualTo("Hi Override Dev | Hello Demo1 Demo"));
        }

        /// <summary>
        /// 異常不會被TargetInvocationException  包裝
        /// </summary>
        [Test, Apartment(ApartmentState.STA)]
        public void Override005()
        {
            var services = new ServiceCollection();
            var demoQueue = new Queue();
            demoQueue.Enqueue("Demo");
            var devQueue = new Queue();
            devQueue.Enqueue("Dev");

            services.AddKeyedSingleton<Queue>("Demo", (sp, name) => demoQueue);
            services.AddKeyedSingleton<Queue>("Dev", (sp, name) => devQueue);
            services.AddScoped<IDemoService, DemoService>();
            services.AddOverride<IDemoService, DemoServiceDev>();

            var provider = services.BuildServiceProvider(true);

            var scope = provider.CreateScope();
            var srv1 = scope.ServiceProvider.GetRequiredService<IDemoService>();
            Assert.Throws<ApplicationException>(() => srv1.SayError());
        }
    }
    namespace Overrides
    {
        public interface IDemoService {
            string SayHello();

            /// <summary>
            /// 觸發異常
            /// </summary>
            /// <returns></returns>
            string SayError();
        }

        public class DemoService([FromKeyedServices("Demo")] Queue queue) : IDemoService
        {
            public string SayError()
            {
                return "This is a Error";
            }

            public string SayHello()
            {
                return $"Hello Demo1 {queue.Peek()}";
            }
        }

        public class DemoServiceDev([FromKeyedServices("Dev")]Queue queue, IDemoService baseone) : IDemoService
        {
            [Override]
            public string SayHello()
            {
                return $"Hi Override {queue.Peek()} | {baseone.SayHello()}";
            }

            [Override]
            public string SayError()
            {
                throw new ApplicationException("Error1");
            }
        }

        public interface ILoginService
        {
            string Login(Login req);

            Task<string> Login2(Login req);

            string Hello();

            string GetName();

            string Account { get; }
        }

        public class LoginService() : ILoginService
        {
            public string Account { get => "david"; }

            public string Login(Login req)
            {
                if (req.Account == "david" && req.Password == "123")
                    return "OK";
                return "Account or Password Error";
            }

            public async Task<string> Login2(Login req)
            {
                await Task.Delay(1000);

                if (req.Account == "david" && req.Password == "123456")
                    return "OK";
                return "Account or Password Error";
            }

            public string Hello()
            {
                return "World";
            }

            public string GetName()
            {
                return "David";
            }
        }

        public class LoginServiceDev(ILoginService basesrv) : ILoginService
        {
            public string Account { get => "kevin"; }

            [Override]
            public string Login(Login req)
            {
                req.Account = "david";
                req.Password = "123";
                return basesrv.Login(req);
            }

            public Task<string> Login2(Login req)
            {
                throw new NotImplementedException();
            }

            [Override]
            public string Hello()
            {
                return basesrv.Hello() + " World Dev";
            }

            public string GetName()
            {
                throw new NotImplementedException();
            }
        }

        public class Login
        {
            public string Account { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        public static class Storage
        {
            public static List<string> Logs { get; } = [];
        }

        public interface IMyLogger<TService> where TService : class
        {
            void LogInfo(TService info);

            void LogWarn(TService info);
        }

        public class MyLoggerImpl<TService> : IMyLogger<TService> where TService : class
        {
            public void LogInfo(TService info)
            {
                Storage.Logs.Add("Info-" + info.ToString());
            }

            public void LogWarn(TService info)
            {
                Storage.Logs.Add("Warn-" + info.ToString());
            }
        }

        public class MyLoggerImpl2<TService>() : IMyLogger<TService> where TService : class
        {

            public void LogInfo(TService info)
            {
            }

            [Override]
            public void LogWarn(TService info)
            {
                //baseOne.LogWarn(info);
                Storage.Logs.Add("Warn-" + info.ToString());
            }
        }

        public class Hello<TService> : DispatchProxy
        {
            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                throw new NotImplementedException();
            }
        }
    }
}
