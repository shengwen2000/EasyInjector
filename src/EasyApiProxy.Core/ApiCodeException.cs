using System.ComponentModel;

namespace EasyApiProxys
{
    /// <summary>
    /// API 發生的異常
    /// </summary>
    public class ApiCodeException : Exception
    {
        /// <summary>
        /// OK or ok 是正常 其他為異常
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// 指定的 HTTP 狀態碼
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// 異常資料
        /// </summary>
        public object? ErrorData { get; set; }

        /// <summary>
        /// 實際呼叫的 URL
        /// </summary>
        public string? TargetUrl { get; set; }

        /// <summary>
        /// 呼叫的方法 (GET/POST...)
        /// </summary>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// 伺服器回傳的追蹤 ID
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// 是否為模型驗證錯誤 (IM)
        /// </summary>
        public bool IsValidationError => Code?.Equals(DefaultApiConstants.Code_IM, StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// 是否為系統異常 (EX)
        /// </summary>
        public bool IsSystemError => Code?.Equals(DefaultApiConstants.Code_EX, StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// API 發生異常
        /// 代號為Enum.ToString()
        /// 訊息自動由 Description中取得 沒有 Description 預設為其名稱
        /// </summary>
        public ApiCodeException(Enum value)
            : this(value, GetDescription(value))
        {
        }

        /// <summary>
        /// API 發生異常
        /// 代號為Enum.ToString()
        /// </summary>
        public ApiCodeException(Enum value, string? message)
             : base(message)
        {
            Code = value.ToString();
            StatusCode = GetHttpStatusCode(value);
        }

        /// <summary>
        /// API 發生異常
        /// 代號為Enum.ToString()
        /// </summary>
        public ApiCodeException(Enum value, string? message, object? errorData)
             : base(message)
        {
            Code = value.ToString();
            ErrorData = errorData;
            StatusCode = GetHttpStatusCode(value);
        }

        /// <summary>
        /// API 發生的異常
        /// <param name="code">錯誤代碼(自動小寫)</param>
        /// <param name="message">錯誤訊息</param>
        /// </summary>
        public ApiCodeException(string code, string? message)
            : base(message)
        {
            Code = code;
        }

        /// <summary>
        /// API 發生的異常
        /// <param name="code">錯誤代碼</param>
        /// <param name="message">錯誤訊息</param>
        /// <param name="errorData">錯誤資料</param>
        /// </summary>
        public ApiCodeException(string code, string? message, object? errorData)
            : base(message)
        {
            Code = code;
            ErrorData = errorData;
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat("{0}->{1}", Code, Message);
            if (ErrorData != null) sb.AppendFormat(" Data:{0}", ErrorData);
            if (!string.IsNullOrEmpty(TargetUrl)) sb.AppendFormat(" Url:[{0}]{1}", HttpMethod, TargetUrl);
            if (!string.IsNullOrEmpty(TraceId)) sb.AppendFormat(" TraceId:{0}", TraceId);
            return sb.ToString();
        }

        /// <summary>
        /// 取得Description
        /// </summary>
        static public string GetDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return value.ToString();

            var attr = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault();
            if (attr != null)
                return attr.Description;
            return value.ToString();
        }

        /// <summary>
        /// 取得 HttpStatusCode
        /// </summary>
        static public int? GetHttpStatusCode(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;

            var attr = fieldInfo.GetCustomAttributes(typeof(ApiStatusCodeAttribute), false)
                .OfType<ApiStatusCodeAttribute>()
                .FirstOrDefault();
            if (attr != null)
                return attr.StatusCode;
            return null;
        }
    }
}
