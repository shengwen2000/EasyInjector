using Microsoft.AspNetCore.Authentication;

namespace BasicAuth.AspNetCore
{
    /// <summary>
    /// Basic Auth 選項（可選，用於配置）
    /// </summary>
    public class BasicAuthorizeOptions : AuthenticationSchemeOptions
    {
        public string Realm { get; set; } = "DefaultAPI"; // Basic Auth 的 Realm

        /// <summary>
        /// 證書清單
        /// </summary>
        public List<BasicCredential> Credentials { get; set; } = [];

        /// <summary>
        /// 取得證書的方法
        /// (預設由 Credentials 取得)
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public Func<string, Task<BasicCredential?>> GetCredentialFunc { get; set; }

        /// <summary>
        /// 驗證選項
        /// </summary>
        public BasicAuthorizeOptions()
        {
            GetCredentialFunc = acct => Task.FromResult(Credentials.Where(x => x.Account == acct)
              .FirstOrDefault());
        }
    }
}