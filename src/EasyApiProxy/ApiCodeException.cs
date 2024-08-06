
namespace EasyApiProxys
{
    /// <summary>
    /// API 發生的異常
    /// </summary>
    public class ApiCodeException : System.Exception
    {
        /// <summary>
        /// OK 是正常 其他為異常
        /// </summary>
        public string Code { get; set; }


        /// <summary>
        /// API 發生的異常
        /// <param name="code">錯誤代碼</param>
        /// <param name="message">錯誤訊息</param>
        /// </summary>
        public ApiCodeException(string code, string message)
            : base(message)
        {
            Code = code;
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}->{1}", Code, Message);
        }
    }
}
