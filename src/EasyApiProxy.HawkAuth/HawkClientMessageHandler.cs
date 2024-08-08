using HawkNet;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EasyApiProxys.HawkAuths
{
    /// <summary>
    /// HawkClientMessageHandler 參考 https://github.com/pcibraro/hawknet/blob/master/HawkNet.WebApi/HawkClientMessageHandler.cs
    /// 因為原本的有個小BUG 所以複製後修改掉
    /// </summary>
    public class HawkClientMessageHandler : DelegatingHandler
    {
        HawkCredential credential;
        string ext;
        DateTime? ts;
        string nonce;
        bool includePayloadHash;

        public HawkClientMessageHandler(HttpMessageHandler innerHandler, HawkCredential credential, string ext = "", DateTime? ts = null, string nonce = null, bool includePayloadHash = false)
            : base(innerHandler)
        {
            if (credential == null ||
                string.IsNullOrEmpty(credential.Id) ||
                string.IsNullOrEmpty(credential.Key) ||
                string.IsNullOrEmpty(credential.Algorithm))
            {
                throw new ArgumentException("Invalid Credential", "credential");
            }

            this.credential = credential;
            this.ext = ext;
            this.ts = ts;
            this.nonce = nonce;
            this.includePayloadHash = includePayloadHash;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            string payloadHash = null;

            if (this.includePayloadHash &&
                request.Method != HttpMethod.Get &&
                request.Content != null)
            {
                payloadHash = Hawk.CalculatePayloadHash(await request.Content.ReadAsStringAsync().ConfigureAwait(false),
                    request.Content.Headers.ContentType.MediaType,
                    credential);
            }

            SignRequest(request, credential,
                this.ext,
                this.ts,
                this.nonce,
                payloadHash);

            return await base.SendAsync(request, cancellationToken)
                .ConfigureAwait(false); //BUG 少了這個
        }

        /// <summary>
        /// Adds the Hawk authorization header to request message
        /// </summary>
        /// <param name="request">Request instance</param>
        /// <param name="credential">Hawk credentials</param>
        /// <param name="ext">Optional extension</param>
        /// <param name="ts">Timestamp</param>
        /// <param name="nonce">Random nonce</param>
        /// <param name="payloadHash">Request payload hash</param>
        void SignRequest(HttpRequestMessage request,
            HawkCredential credential,
            string ext = null,
            DateTime? ts = null,
            string nonce = null,
            string payloadHash = null)
        {
            var host = (request.Headers.Host != null) ? request.Headers.Host :
                request.RequestUri.Host +
                    ((request.RequestUri.Port != 80) ? ":" + request.RequestUri.Port : "");

            var hawk = Hawk.GetAuthorizationHeader(host,
                request.Method.ToString(),
                request.RequestUri,
                credential,
                ext,
                ts,
                nonce,
                payloadHash);

            request.Headers.Authorization = new AuthenticationHeaderValue("Hawk", hawk);
        }
    }
}
