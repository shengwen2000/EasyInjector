using Castle.DynamicProxy;
using System;

namespace EasyApiProxys
{
    public class ApiProxyBuilder
    {
        public ApiProxyOptions Options { get; set; }

        public ApiProxyBuilder()
        {
            Options = new ApiProxyOptions();
        }

        /// <summary>
        /// 建立的API 應該要重複使用，而不是只用一次就不用
        /// 因為WebApi 依賴 HttpClient
        /// HttpClient 應該只建立一份並重複利用。為每次請求建立新的 HttpClient，重度使用下可能用光 Socket Port 導致 SocketException。以下是正確的寫法：
        /// </summary>
        public TApi Build<TApi>() where TApi : class
        {
            var generator = new ProxyGenerator();
            var inteceptor1 = new ApiProxyInterceptor<TApi>(Options);
            var api = generator.CreateInterfaceProxyWithoutTarget<TApi>(inteceptor1);
            return api;
        }       
    }
}
