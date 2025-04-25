using System.Net.Http.Headers;

namespace EasyApiProxys.BasicAuth;

/// <summary>
/// HttpClient's message handler for Basic authentication.
/// </summary>
public class BasicClientMessageHandler : DelegatingHandler
{
    readonly AuthenticationHeaderValue authHeader;

    public BasicClientMessageHandler(HttpMessageHandler innerHandler, BasicCredential credential)
        : base(innerHandler)
    {
        if (credential == null ||
            string.IsNullOrEmpty(credential.Account))
        {
            throw new ArgumentException("Invalid Credential", nameof(credential));
        }

        this.authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{credential.Account}:{credential.PassCode}")));
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
        request.Headers.Authorization = authHeader;
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}