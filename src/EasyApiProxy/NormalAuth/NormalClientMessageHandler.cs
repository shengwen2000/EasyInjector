using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EasyApiProxys.NormalAuth
{
    /// <summary>
    /// HttpClient's message handler for Basic authentication.
    /// </summary>
    public class NormalClientMessageHandler : DelegatingHandler
    {
        readonly AuthenticationHeaderValue authHeaderValue;

        /// <summary>
        ///   HttpClient's message handler for Basic authentication.
        ///   <param name="authHeaderValue">e.g. Basic QVBJOjg1MTU2MTE0MzE1MTYxNjg=</param>
        ///   <param name="innerHandler">inner Handler</param>
        /// </summary>   
        public NormalClientMessageHandler(HttpMessageHandler innerHandler, AuthenticationHeaderValue authHeaderValue)
            : base(innerHandler)
        {
            this.authHeaderValue = authHeaderValue;
        }

        /// <summary>
        /// 作為異步操作，將 HTTP 請求發送到內部處理程序以發送到服務器。
        /// 在發送請求前添加 Basic 認證頭。
        /// </summary>
        /// <param name="request">要發送到服務器的 HTTP 請求消息。</param>
        /// <param name="cancellationToken">用於取消操作的取消標記。</param>
        /// <returns>從服務器接收的 HTTP 響應消息。</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = authHeaderValue;
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}