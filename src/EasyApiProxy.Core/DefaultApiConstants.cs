
namespace EasyApiProxys
{
    /// <summary>
    /// DefaultApi 協議相關常量
    /// </summary>
    public static class DefaultApiConstants
    {
        #region Headers

        /// <summary>
        /// Header Name 回應代號 X-Api-Result
        /// </summary>
        public const string Header_Result = "X-Api-Result";

        /// <summary>
        /// Header Name 資料型別 X-Api-DataType
        /// </summary>
        public const string Header_DataType = "X-Api-DataType";

        /// <summary>
        /// 舊版 Header Name 回應代號 X_Api_Result (違背規範)
        /// </summary>
        public const string Header_Result_Legacy = "X_Api_Result";

        /// <summary>
        /// 舊版 Header Name 資料型別 X_Api_DataType (違背規範)
        /// </summary>
        public const string Header_DataType_Legacy = "X_Api_DataType";

        #endregion

        #region Result Codes

        /// <summary>
        /// 執行成功
        /// </summary>
        public const string Code_OK = "OK";

        /// <summary>
        /// 模型驗證失敗 (Input Mistake)
        /// </summary>
        public const string Code_IM = "IM";

        /// <summary>
        /// 系統執行異常 (Exception)
        /// </summary>
        public const string Code_EX = "EX";

        /// <summary>
        /// 非預期的通訊內容格式
        /// </summary>
        public const string Code_NonDefault = "NON_DEFAULT_API_RESULT";

        #endregion
    }
}
