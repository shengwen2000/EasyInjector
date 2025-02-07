
namespace EasyApiProxys
{
    /// <summary>
    /// The Builder 建立 Api Proxy Factory
    /// </summary>
    public class ApiProxyBuilder
    {
        /// <summary>
        /// Proxy建立選項
        /// </summary>
        public ApiProxyBuilderOptions Options { get; set; }

        /// <summary>
        /// The Builder 建立 Api Proxy Factory
        /// </summary>
        public ApiProxyBuilder()
        {
            Options = new ApiProxyBuilderOptions();
        }

        /// <summary>
        /// 建立的 FACTORY 應該要重複使用，而不是只用一次就不用
        /// 因為每個FACTORY 都綁定一個新的HTTPClient 所建立的 Proxy 都共用同一份 HttpClient實例
        /// 若每次請求建立新的 Facotry，重度使用下可能用光 Socket Port 導致 SocketException。
        /// </summary>
        public IApiProxyFactory<TAPI> Build<TAPI>() where TAPI : class
        {
            // 預設最後套用 InstanceCall Handler
            InstanceCallExtension.UseInstanceCallHandler(this);

            return new ApiProxyFactory<TAPI>(Options);
        }
    }
}
