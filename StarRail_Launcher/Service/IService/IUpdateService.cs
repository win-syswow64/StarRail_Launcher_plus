using StarRail_Launcher.Helper;
using StarRail_Launcher.ViewModels;

namespace StarRail_Launcher.Service.IService
{
    public interface IUpdateService
    {
        /// <summary>
        /// 检查更新服务
        /// </summary>
        /// <param name="main">MainView的实例</param>
        /// <returns></returns>
        void CheckUpdate(MainWindow main);

        /// <summary>
        /// 运行更新服务
        /// </summary>
        /// <param name="vm">UpdatePageViewModel的实例</param>
        /// <returns></returns>
        void UpdateRun(UpdatePageViewModel vm);

    }
}
