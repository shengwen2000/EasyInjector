
namespace EasyApiProxys
{
    /// <summary>
    /// Default Api Protocl 回應封裝格式
    /// </summary>
    public class DefaultApiResult
    {
        /// <summary>
        /// 回傳代碼 OK 正常 其他為例外代碼
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 回應訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 回傳資料
        /// </summary>
        protected object _data;

        /// <summary>
        /// 回傳資料
        /// </summary>
        public object Data {
            get { return _data; }
            set { _data = value; }
        }
    }

    /// <summary>
    /// Default Api Protocl 回應封裝格式 (StrongTyped)
    /// </summary>
    public class DefaultApiResult<T> : DefaultApiResult
    {
        /// <summary>
        /// 回傳資料
        /// </summary>
        new public T Data
        {
            get { return (T)_data; }
            set { _data = value; }
        }
    }
}
