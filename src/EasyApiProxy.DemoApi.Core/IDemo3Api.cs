using System.ComponentModel.DataAnnotations;

namespace EasyApiProxys.DemoApis
{
    public interface IDemo3Api
    {
        /// <summary>
        /// IM 全域指定為 541
        /// - ValidationException
        /// </summary>
        Task ErrorG1();

        /// <summary>
        /// IM 全域指定為 561
        /// - 未指定例外 ArgumentException
        /// </summary>
        Task ErrorG2();

        /// <summary>
        /// IM 全域沒有指定為 為 200
        /// - 未指定例外 ArgumentException
        /// </summary>
        Task ErrorG3();

        /// 測試 舊版 Header 啟用
        int LegacyHeaderEnabled();

        /// 測試 舊版 Header 停用
        int LegacyHeaderDisabled();
    }
}
