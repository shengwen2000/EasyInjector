
using System;
using System.ComponentModel;
using System.Linq;
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
        /// 異常資料
        /// </summary>
        public object ErrorData { get; set; }

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
        public ApiCodeException(Enum value, string message)
            : base(message)
        {
            Code = value.ToString();
        }

        /// <summary>
        /// API 發生異常
        /// 代號為Enum.ToString()
        /// </summary>
        public ApiCodeException(Enum value, string message, object errorData)
            : base(message)
        {
            Code = value.ToString();
            ErrorData = errorData;
        }


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
            if (ErrorData != null)
                return string.Format("{0}->{1} {2}", Code, Message, ErrorData);
            else
                return string.Format("{0}->{1}", Code, Message);
        }

        /// <summary>
        /// API 發生的異常
        /// <param name="code">錯誤代碼</param>
        /// <param name="message">錯誤訊息</param>
        /// <param name="errorData">錯誤資料</param>
        /// </summary>
        public ApiCodeException(string code, string message, object errorData)
            : base(message)
        {
            Code = code;
            ErrorData = errorData;
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
    }
}
