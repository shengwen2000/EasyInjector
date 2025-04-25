
namespace BasicAuth.AspNetCore;

/// <summary>
/// Basic Credential
/// </summary>
public class BasicCredential
{
    /// <summary>
    /// 帳號 (區分大小寫)
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 密碼 (區分大小寫)
    /// </summary>
    public string PassCode { get; set; } = string.Empty;

    /// <summary>
    /// 驗證通過給予的 User name
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// 驗證通過給予的 User name User roles
    /// </summary>

    public string[] Roles { get; set; } = [];
}