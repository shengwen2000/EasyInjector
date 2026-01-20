using System.ComponentModel.DataAnnotations;

namespace EasyApiProxys.DemoApis
{
    public interface IDemo2Api
    {
        /// <summary>
        /// 呼叫會引發一般例外
        /// - 列舉指定回傳狀態碼
        /// </summary>
        Task Error1();

        /// <summary>
        /// 方法異常指定回傳狀態碼
        /// - InvalidOperationException -> 409
        /// </summary>
        Task Error2();

        /// <summary>
        /// 類別異常指定回傳狀態碼
        /// - ApplicationException -> 503
        /// </summary>
        Task Error2A();

        /// <summary>
        /// 全域異常指定回傳狀態碼
        /// - NotImplementedException -> 504
        /// </summary>
        Task Error2B();

        /// <summary>
        /// 類別異常優先於全域指定回傳狀態碼
        /// - NotSupportedException -> 525
        /// </summary>
        Task Error2C();

        /// <summary>
        /// 方法優先於類別異常優先於全域指定回傳狀態碼
        /// - NotSupportedException -> 535
        /// </summary>
        Task Error2D();

        /// <summary>
        /// IM 全域指定為 541
        /// - ValidationException
        /// </summary>
        //Task Error3();

        /// <summary>
        /// IM 類別指定為 542
        /// </summary>
        Task Error3A();

         /// <summary>
        /// IM 方法指定為 543
        /// - 優先於類別
        /// </summary>
        Task Error3B();
    }


}
