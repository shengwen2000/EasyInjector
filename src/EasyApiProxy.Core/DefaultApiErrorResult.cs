
using System.Text.Json.Serialization;

namespace EasyApiProxys
{
    /// <summary>
    /// Default Api 錯誤回應格式
    /// </summary>
    public class DefaultApiErrorResult
    {
        /// <summary>
        /// apiResult builder
        /// </summary>
        public static DefaultApiErrorResult From(string result, string? message)
        {
            return new DefaultApiErrorResult { Result = result, Message = message };
        }

        /// <summary>
        /// apiResult builder
        /// </summary>
        public static DefaultApiErrorResult From(string result, string? message, object? data)
        {
            return new DefaultApiErrorResult { Result = result, Message = message, Data = data };
        }

        /// <summary>
        /// apiResult builder
        /// </summary>
        public static DefaultApiErrorResult<T> From<T>(string result, string? message, T? data)
        {
            return new DefaultApiErrorResult<T> { Result = result, Message = message, Data = data };
        }

        /// <summary>
        /// apiResult builder
        /// </summary>
        public static DefaultApiErrorResult FromException(ApiCodeException apiCodeException)
        {
            return new DefaultApiErrorResult { Result = apiCodeException.Code, Message = apiCodeException.Message };
        }

        /// <summary>
        /// 回傳代碼 ok 正常 其他為例外代碼
        /// </summary>
        [JsonPropertyOrder(1)]
        public string Result { get; set; } = default!;

        /// <summary>
        /// 回應訊息
        /// </summary>
        [JsonPropertyOrder(2)]
        public string? Message { get; set; }

        /// <summary>
        /// 回傳資料
        /// </summary>
        protected object? _data;

        /// <summary>
        /// 回傳資料
        /// </summary>
        [JsonPropertyOrder(3)]
        public object? Data {
            get { return _data; }
            set { _data = value; }
        }
    }

    /// <summary>
    /// Default Api Protocol 錯誤回應封裝格式 (StrongTyped)
    /// </summary>
    public class DefaultApiErrorResult<T> : DefaultApiErrorResult
    {
        /// <summary>
        /// 回傳資料
        /// </summary>
        [JsonPropertyOrder(3)]
        new public T? Data
        {
            get {
                if (_data == null) return default;
                return (T) _data;
            }
            set { _data = value; }
        }
    }
}
