﻿using StarRail_Launcher.ViewModels;

namespace StarRail_Launcher.Service.IService
{
    public interface IMainWindowService
    {
        /// <summary>
        /// 检查配置文件
        /// </summary>
        /// <param name="main">MainView的实例</param>
        /// <returns></returns>
        void CheckConfig(MainWindow main);

        /// <summary>
        /// 加载背景页面
        /// </summary>
        /// <param name="vm">MainWindowViewModel的实例</param>
        /// <returns></returns>
        void MainBackgroundLoad(MainWindowViewModel vm);
    }
}
