﻿using System.ComponentModel;
namespace EasyApiProxys
{
    /// <summary>
    /// API 發生的異常
    /// </summary>
    public class ApiCodeException : Exception
    {
        /// <summary>
        /// OK 是正常 其他為異常
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// API 發生異常
        /// 代號為Enum.ToString()
        /// 訊息自動由 Description中取得 沒有 Description 預設為其名稱
        /// </summary>
        public ApiCodeException(Enum value)
            : this(value.ToString(), GetDescription(value))
        {
        }

        /// <summary>
        /// API 發生異常
        /// 代號為Enum.ToString()
        /// </summary>
        public ApiCodeException(Enum value, string message)
            : this(value.ToString(), message)
        {
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
            return string.Format("{0}->{1}", Code, Message);
        }

        /// <summary>
        /// 取得Description
        /// </summary>
        static string GetDescription(Enum value)
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