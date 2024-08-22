using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HawkNet.AspNetCore;

/// <summary>
/// Hawk 驗證
/// - 注意 因考量性能因素不支援 payload hash
/// </summary>
public class HawkAuthorizeHanlder(
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptionsMonitor<HawkAuthorizeOptions> options) : AuthenticationHandler<HawkAuthorizeOptions>(options, logger, encoder)
{
    /// <summary>
    /// 預設的Scheme名稱 (Hawk)
    /// </summary>
    public const string AuthenticationScheme = "Hawk";
    private readonly HawkAuthorizeOptions _options = options.CurrentValue;

    /// <summary>
    /// 驗證授權
    /// </summary>
    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // parse Token
        foreach (var auth in Context.Request.Headers.Authorization)
        {
            if (auth == null)
                continue;

            // not hawk do nothing
            if (!auth.StartsWith(AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
                continue;

            // 有授權內容
            // e.g. id="medicalapp", ts="1717405011", nonce="WSQx1z", mac="8oljTeAyjbvfK4FJRfDeAklC176Xh3DhkO+EnnnqbwY="
            var tokenText = auth[5..];
            try
            {
                var url = new Uri(Context.Request.GetDisplayUrl());
                var principal = await Hawk.AuthenticateAsync(
                    authorization: tokenText,
                    method: Context.Request.Method,
                    uri: url,
                    getCredentialFunc: _options.GetCredentialFunc,
                    timestampSkewSec: _options.TimestampSkewSec
                    );

                return AuthenticateResult.Success(new AuthenticationTicket(principal, AuthenticationScheme));
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
        }
        return AuthenticateResult.NoResult();
    }
}
