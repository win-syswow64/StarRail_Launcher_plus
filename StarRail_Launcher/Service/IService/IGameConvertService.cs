using StarRail_Launcher.ViewModels;
using System.Threading.Tasks;

namespace StarRail_Launcher.Service.IService
{
    public interface IGameConvertService
    {

        /// <summary>
        /// 异步转换游戏客户端文件
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        Task ConvertGameFileAsync(SettingsPageViewModel vm);


        /// <summary>
        /// 保存游戏客户端配置
        /// </summary>
        /// <param name="vm"></param>
        void SaveGameConfig(SettingsPageViewModel vm);
    }
}
