namespace HawkNet.AspNetCore;

/// <summary>
/// 參考 https://github.com/pcibraro/hawknet 將Hawk 核心程式碼移轉過來
/// (主要是原作者沒有繼續在維護的樣子)
/// Contains private information about an user
/// </summary>
public class HawkCredential
{
    /// <summary>
    /// Key Id
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Symmetric Key
    /// </summary>
    public string Key { get; set; } = default!;

    /// <summary>
    /// Hashing Algorithm sha1 or sha256
    /// </summary>
    public string Algorithm { get; set; } = "sha256";

    /// <summary>
    /// 驗證通過給予的 User name
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// 驗證通過給予的 User name User roles
    /// </summary>
    public string[] Roles { get; set; } = [];

}