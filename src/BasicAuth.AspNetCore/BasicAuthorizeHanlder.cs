using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace BasicAuth.AspNetCore
{

    /// <summary>
    /// Basic Auth 授權處理
    /// </summary>
    public class BasicAuthorizeHanlder(
        IOptionsMonitor<BasicAuthorizeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<BasicAuthorizeOptions>(options, logger, encoder)
    {

        private readonly BasicAuthorizeOptions _options = options.CurrentValue;

        public const string AuthenticationScheme = "Basic";

        /// <summary>
        /// 驗證授權
        /// </summary>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            await Task.CompletedTask;

            foreach (var auth in Context.Request.Headers.Authorization)
            {
                if (auth == null)
                    continue;

                // not basic
                if (auth.StartsWith("Basic", StringComparison.OrdinalIgnoreCase) == false)
                    continue;

                // 解碼 Base64 憑證（格式：username:password）
                try
                {
                    var authValue = auth["Basic ".Length..].Trim();
                    var credentialBytes = Convert.FromBase64String(authValue);
                    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                    if (credentials.Length != 2)
                        return AuthenticateResult.Fail("Invalid credentials format.");

                    var username = credentials[0];
                    var password = credentials[1];

                    var credential = await _options.GetCredentialFunc(username);
                    if (credential == null || credential.PassCode != password)
                        return AuthenticateResult.Fail("Invalid username or password.");

                    var claimIdentity = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, credential.User),
                        ..credential.Roles.Select(role => new Claim(ClaimTypes.Role, role)),
                    ], AuthenticationScheme);
                    var claimPrincipal = new ClaimsPrincipal(claimIdentity);
                    return AuthenticateResult.Success(new AuthenticationTicket(claimPrincipal, AuthenticationScheme));
                }
                catch (Exception ex)
                {
                    return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
                }
            }
            return AuthenticateResult.NoResult();
        }

        // 處理未驗證的情況，返回 401 並帶 WWW-Authenticate 標頭
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers.WWWAuthenticate = $"Basic realm=\"{Options.Realm}\"";
            Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    }
}