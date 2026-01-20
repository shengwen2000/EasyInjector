using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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

        /// <summary>
        /// 忽略此方法，不進行 API 封裝
        /// - 回傳 571 Ignore It
        /// </summary>
        string IgnoreIt();
    }
}
