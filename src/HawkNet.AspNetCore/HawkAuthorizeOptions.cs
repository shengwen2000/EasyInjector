using Microsoft.AspNetCore.Authentication;

namespace HawkNet.AspNetCore;

/// <summary>
/// Hawk驗證選項
/// </summary>
public class HawkAuthorizeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// 證書清單
    /// </summary>
    public List<HawkCredential> Credentials { get; set; } = [];

    /// <summary>
    /// 取得證書的方法
    /// (預設由 Credentials 取得)
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Func<string, Task<HawkCredential?>> GetCredentialFunc { get; set;}

    /// <summary>
    /// 允許多少時間誤差(單位秒) Hawk驗證有時間驗證，如通訊雙方時間無差超過此數則驗證失敗。
    /// (default 120 seconds)
    /// </summary>
    public int TimestampSkewSec { get; set; } = 120;

    /// <summary>
    /// Hawk驗證選項
    /// </summary>
    public HawkAuthorizeOptions()
    {
        GetCredentialFunc = id => Task.FromResult(Credentials.Where(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
          .FirstOrDefault());
    }
}