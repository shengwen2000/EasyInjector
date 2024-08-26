using System.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Tests.Dependencys;

namespace Tests
{
    [TestFixture]
    public class DependencyTest : BaseTest
    {
        [Test, Apartment(ApartmentState.STA)]
        public void Dependency001()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService1, Service1>();
            {
                var srvType1 = services.Where(x => x.ServiceType == typeof(IService1))
                .LastOrDefault();
                Assert.That(srvType1, Is.Not.Null);
                Assert.That(srvType1.ImplementationFactory, Is.Null);
                Assert.That(srvType1.ImplementationType, Is.Not.Null);
            }
            services.AddSingleton<IService1>(sp => new Service1());
            {
                var srvType1 = services.Where(x => x.ServiceType == typeof(IService1))
                .LastOrDefault();
                Assert.That(srvType1, Is.Not.Null);
                Assert.That(srvType1.ImplementationFactory, Is.Not.Null);
                Assert.That(srvType1.ImplementationType, Is.Null);
            }

            services.AddSingleton(typeof(IServiceA<>), typeof(ServiceA<>));
            {
                var srvType1 = services.Where(x => x.ServiceType == typeof(IServiceA<>))
                    .LastOrDefault();
                Assert.That(srvType1, Is.Not.Null);
                Assert.That(srvType1.ImplementationType, Is.Not.Null);
            }

            var provider = services.BuildServiceProvider(true);

            var srv1 = provider.GetRequiredService<IServiceA<string>>();

            var srvprovider = provider as Microsoft.Extensions.DependencyInjection.ServiceProvider;

            Assert.That(typeof(string).Name, Is.EqualTo(srv1.GetTypeName()));

            //ServiceIdentifier.FromServiceType(serviceType)
        }


        [Test, Apartment(ApartmentState.STA)]
        public void Dependency002()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IService1, Service1>();
            services.AddSingleton<IService1, Service1B>();

            var provider = services.BuildServiceProvider(true);

            var p1 = provider.GetServices<IService1>();
            {
                var srv1b = provider.GetRequiredService<IService1>();
                Assert.That(srv1b, Is.AssignableFrom<Service1B>());

                var srv1 = GetPreviousService(provider, typeof(IService1));
                Assert.That(srv1, Is.AssignableFrom<Service1>());
            }

            using (var scope = provider.CreateScope())
            {
                var srv1 = GetPreviousService(scope.ServiceProvider, typeof(IService1));
                Assert.That(srv1, Is.AssignableFrom<Service1>());
            }
        }

        /// <summary>
        /// 取得上一個註冊的服務
        /// </summary>
        static object? GetPreviousService(IServiceProvider provider, Type serviceType)
        {
            var dd = GetServiceDescriptors(provider)
                .Where(x => x.ServiceType == serviceType)
                .ToArray();
            if (dd.Length <= 1) return null;

            var prv1 = provider.GetServices(serviceType)
                .Skip(dd.Length - 2)
                .First();
            return prv1;
        }

        static IList<ServiceDescriptor> GetServiceDescriptors(IServiceProvider provider)
        {
            if (provider is ServiceProvider sp1)
                return GetServiceDescriptors(sp1);

            // 可能是Scope
            else
            {
                var prop1 = provider.GetType().GetProperty("RootProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?? throw new NotSupportedException();
                var sp2 = prop1.GetValue(provider) as ServiceProvider
                    ?? throw new NotSupportedException();

                return GetServiceDescriptors(sp2);
            }

            static IList<ServiceDescriptor> GetServiceDescriptors(ServiceProvider sp)
            {
                var f1 = typeof(ServiceProvider).GetProperty("CallSiteFactory", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?? throw new NotSupportedException();
                var v1 = f1.GetValue(sp) ?? throw new NotSupportedException();
                var f2 = v1.GetType().GetField("_descriptors", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?? throw new NotSupportedException();
                var v2 = f2.GetValue(v1);
                var ss2 = v2 as IList<ServiceDescriptor> ?? throw new NotSupportedException();
                return ss2;
            }
        }
    }


    namespace Dependencys
    {
        public interface IService1
        {
            string Hello();
        }

        public class Service1 : IService1
        {
            public string Hello()
            {
                return "World";
            }
        }

        public class Service1B : IService1
        {
            public string Hello()
            {
                return "World2";
            }
        }

        public interface IServiceA<TType>
        {
            string GetTypeName();
        }

        public class ServiceA<TType> : IServiceA<TType>
        {
            public string GetTypeName()
            {
                return typeof(TType).Name;
            }
        }
    }
}
