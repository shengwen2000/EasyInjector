using Castle.DynamicProxy;

namespace KmuApps.ApiProxys
{
    public class ApiProxyBuilder
    {
        public ApiProxyOptions Options { get; set; }

        public ApiProxyBuilder()
        {
            Options = new ApiProxyOptions();
        }

        public TApi Build<TApi>() where TApi : class
        {
            var generator = new ProxyGenerator();
            var inteceptor1 = new ApiProxyInterceptor<TApi>(Options);
            var api = generator.CreateInterfaceProxyWithoutTarget<TApi>(inteceptor1);
            return api;
        }
    }
}
