﻿using StarRail_Launcher.Service.IService;
using StarRail_Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using StarRail_Launcher.Helper;

namespace StarRail_Launcher.Service
{
    internal class ConvertService : IGameConvertService
    {
        private const string CN_DIRECTORY = "CnFile";

        private const string GLOBAL_DIRECTORY = "GlobalFile";
        // private const string YUANSHEN_DATA = "YuanShen_Data";
        // private const string GENSHINIMPACT_DATA = "GenshinImpact_Data";
        // private const string YUANSHEN_EXE = "YuanShen.exe";
        // private const string GENSHINIMPACT_EXE = "GenshinImpact.exe";

        private const string STARRAIL_DATA = "StarRail_Data";
        private const string STARRAIL_EXE = "StarRail.exe";

        private string GamePath { get; set; }
        private string Scheme { get; set; }
        private string PkgPerfix { get; set; }
        private string GameSource { get; set; }
        private string GameDest { get; set; }
        private string CurrentPath { get; set; }
        private string ReplaceSourceDirectory { get; set; }
        private string RestoreSourceDirectory { get; set; }
        private List<string> GameFileList { get; set; }

        public ConvertService()
        {
            GamePath = App.Current.DataModel.GamePath;
            CurrentPath = Environment.CurrentDirectory;
        }

        /// <summary>
        /// 检查Pkg版本
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="vm"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// 

        public bool CheckPackageVersion(string scheme, SettingsPageViewModel vm)
        {
            string pkgfile = App.Current.UpdateObject.PkgVersion;
            if (!File.Exists($"{scheme}/{pkgfile}"))
            {
                vm.ConvertingLog = $"{App.Current.Language.NewPkgVer} : [{pkgfile}]\r\n";
                ProcessStartInfo info = new()
                {
                    FileName = "http://pan.115832958.xyz:25212/s/85f3",
                    UseShellExecute = true,
                };
                 Process.Start(info);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 异步转换游戏任务
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task ConvertGameFileAsync(SettingsPageViewModel vm)
        {
            GameFileList = new List<string>();
            Scheme = GetCurrentSchemeName();
            PkgPerfix = Scheme == CN_DIRECTORY ? GLOBAL_DIRECTORY : CN_DIRECTORY;
            ReplaceSourceDirectory = Scheme == CN_DIRECTORY ? GLOBAL_DIRECTORY : CN_DIRECTORY;
            RestoreSourceDirectory = Scheme == CN_DIRECTORY ? CN_DIRECTORY : GLOBAL_DIRECTORY;

            await Task.Run(async () =>
            {
                if (File.Exists(Path.Combine(GamePath, $"{STARRAIL_EXE}.cn.bak")) ||
                    File.Exists(Path.Combine(GamePath, $"{STARRAIL_EXE}.global.bak")))
                {
                    //直接从 bak 还原
                    vm.StateIndicator = "正在获取备份清单";
                    if (Directory.Exists(Path.Combine(CurrentPath, RestoreSourceDirectory)))
                    {
                        await GetFilesName(Path.Combine(CurrentPath, RestoreSourceDirectory));
                        vm.ConvertingLog += $"正在还原客户端\r\n";
                        await RestoreGameFiles(vm);
                    }
                    else
                    {
                        vm.ConvertingLog += $"转换Pkg副本已丢失，无法还原\r\n";
                        vm.ConvertState = false;
                    }
                }
                else if (Directory.Exists(Path.Combine(CurrentPath, PkgPerfix)))
                {
                    //直接从 pkg解压后的目录 处替换
                    bool up = CheckPackageVersion(ReplaceSourceDirectory, vm);
                    if (!up)
                    {
                        Directory.Delete($"{CurrentPath}/{ReplaceSourceDirectory}", true);
                        vm.ConvertState = false;
                        return;
                    }

                    vm.StateIndicator = "正在获取文件清单";
                    await GetFilesName(Path.Combine(CurrentPath, ReplaceSourceDirectory));
                    await ReplaceGameFiles(vm);
                }
                else if (File.Exists(Path.Combine(CurrentPath, $"{PkgPerfix}.pkg")))
                {
                    vm.StateIndicator = "开始解压Pkg文件";
                    //解压 pkg 文件
                    if (Decompress(PkgPerfix))
                    {
                        bool up = CheckPackageVersion(ReplaceSourceDirectory, vm);
                        if (!up)
                        {
                            vm.ConvertState = false;
                            Directory.Delete($"{CurrentPath}/{ReplaceSourceDirectory}", true);
                            return;
                        }

                        //直接从 pkg解压后的目录 处替换
                        vm.StateIndicator = "正在获取文件清单";
                        await GetFilesName(Path.Combine(CurrentPath, ReplaceSourceDirectory));
                        await ReplaceGameFiles(vm);
                    }
                    else
                    {
                        vm.ConvertingLog += $"{PkgPerfix}.pkg 文件解压失败\r\n";
                        vm.ConvertState = false;
                    }
                }
                else
                {
                    vm.ConvertingLog += $"{PkgPerfix}.pkg 文件不存在\r\n";
                    vm.ConvertState = false;
                }

                vm.StateIndicator = "无状态";
            });
        }

        /// <summary>
        /// 获取所有文件加入到清单
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task GetFilesName(string directory)
        {
            try
            {
                DirectoryInfo directoryInfo = new(directory);
                FileInfo[] files = directoryInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temp =
                        file.FullName.Replace(Path.Combine(Environment.CurrentDirectory, ReplaceSourceDirectory), "");
                    GameFileList.Add(temp);
                }

                DirectoryInfo[] dirs = directoryInfo.GetDirectories();
                if (dirs.Length > 0)
                {
                    foreach (DirectoryInfo dir in dirs)
                    {
                        GetFilesName(dir.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 获取当前需要的Pkg前缀
        /// </summary>
        /// <returns></returns>
        public string GetCurrentSchemeName()
        {
            // if (File.Exists(Path.Combine(GamePath, YUANSHEN_EXE)))
            // {
            //     return CN_DIRECTORY;
            // }
            // else if (File.Exists(Path.Combine(GamePath, GENSHINIMPACT_EXE)))
            // {
            //     return GLOBAL_DIRECTORY;
            // }
            switch (App.Current.DataModel.Cps)
            {
                case "gw_PC":
                    return CN_DIRECTORY;
                case "hoyoverse_PC":
                    return GLOBAL_DIRECTORY;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 替换游戏文件逻辑
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public async Task ReplaceGameFiles(SettingsPageViewModel vm)
        {
            vm.ConvertingLog += "开始备份文件\r\n";
            await BackupGameFile(vm);
            // vm.ConvertingLog += $"原目录：{Path.Combine(GamePath, GameSource)}\r\n";
            // vm.ConvertingLog += $"新目录：{Path.Combine(GamePath, GameDest)}\r\n";
            // Directory.Move(Path.Combine(GamePath, GameSource), Path.Combine(GamePath, GameDest));
            // 备份完毕开始替换
            vm.StateIndicator = "开始替换客户端";
            vm.ConvertingLog += "释放Pkg文件至游戏目录\r\n";
            foreach (string file in GameFileList)
            {
                string temp = file.Replace(@$"\{STARRAIL_DATA}", STARRAIL_DATA);
                string gameFilePath = temp.Insert(0, $@"{GamePath}\");
                string pkgFilePath = temp.Insert(0,
                    $@"{Path.Combine(Environment.CurrentDirectory, ReplaceSourceDirectory)}\");
                if (File.Exists(pkgFilePath))
                {
                    try
                    {
                        File.Copy(pkgFilePath, gameFilePath, true);
                        vm.ConvertingLog += $"{pkgFilePath} 替换成功\r\n";
                    }
                    catch (Exception ex)
                    {
                        vm.ConvertingLog += $"警告：{ex.Message} \r\n";
                    }
                }
                else
                {
                    vm.ConvertingLog += $"{gameFilePath}替换失败，文件有所缺失\r\n";
                }
            }

            string cps = Scheme == CN_DIRECTORY ? "gw_PC" : "hoyoverse_PC";
            vm.IsMihoyo = cps == "hoyoverse_PC" ? 0 : 2;
            vm.ConvertingLog += $"所有文件替换完成，尽情享受吧...\r\n";
            vm.ConvertState = true;
            // App.Current.DataModel.Cps = Scheme != CN_DIRECTORY ? "gw_PC" : "hoyoverse_PC";
        }


        /// <summary>
        /// 还原游戏文件
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public async Task RestoreGameFiles(SettingsPageViewModel vm)
        {
            vm.StateIndicator = "开始还原文件";
            string suffix = Scheme == GLOBAL_DIRECTORY ? "cn" : "global";

            vm.ConvertingLog += "开始还原文件\r\n";
            foreach (string file in GameFileList)
            {
                string temp = file.Replace(Path.Combine(CurrentPath, RestoreSourceDirectory), "");
                string filePath = temp.Insert(0, $@"{GamePath}");
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        File.Move($@"{filePath}.{suffix}.bak", filePath);
                        vm.ConvertingLog += $"{filePath} 还原成功\r\n";
                    }
                    catch (Exception e)
                    {
                        vm.ConvertingLog += $"警告：{e.Message} \r\n";
                    }
                }
                else
                {
                    vm.ConvertingLog += $"{filePath}还原失败，文件不存在\r\n";
                }
            }

            // vm.ConvertingLog += $"新游戏本体路径:{Path.Combine(GamePath, $"{gamee}")} \r\n";
            // File.Move(Path.Combine(GamePath, $"{STARRAIL_EXE}.{suffix}.bak"), Path.Combine(GamePath, STARRAIL_EXE));
            // Directory.Move(Path.Combine(GamePath, GameSource), Path.Combine(GamePath, GameDest));

            string cps = Scheme == CN_DIRECTORY ? "gw_PC" : "hoyoverse_PC";
            vm.IsMihoyo = cps == "hoyoverse_PC" ? 0 : 2;
            vm.ConvertingLog += $"所有文件还原完成，尽情享受吧...\r\n";
            vm.ConvertState = true;
            // App.Current.DataModel.Cps = Scheme != CN_DIRECTORY ? "gw_PC" : "hoyoverse_PC";
        }

        /// <summary>
        /// 备份原来的游戏文件
        /// </summary>
        /// <returns></returns>
        public async Task BackupGameFile(SettingsPageViewModel vm)
        {
            vm.StateIndicator = "开始备份文件";
            string suffix = Scheme == GLOBAL_DIRECTORY ? "global" : "cn";

            foreach (string file in GameFileList)
            {
                string temp = file.Replace(@$"\{STARRAIL_DATA}", STARRAIL_DATA);
                string filePath = temp.Insert(0, $@"{GamePath}\");
                if (File.Exists(filePath))
                {
                    try
                    {
                        
                        File.Move(filePath, $@"{filePath}.{suffix}.bak");
                        vm.ConvertingLog += $"{filePath} 备份成功\r\n";
                    }
                    catch (Exception ex)
                    {
                        vm.ConvertingLog += $"警告：{ex.Message} \r\n";
                    }
                }
                else
                {
                    vm.ConvertingLog += $"{filePath}备份失败，文件不存在\r\n";
                }
            }

            // vm.ConvertingLog += $"游戏本体exe:{STARRAIL_EXE} \r\n";
            vm.ConvertingLog += $"原游戏本体路径:{Path.Combine(GamePath, STARRAIL_EXE)} \r\n";
            vm.ConvertingLog += $"新游戏本体路径:{Path.Combine(GamePath, $"{STARRAIL_EXE}.{suffix}.bak")} \r\n";
            // File.Move(Path.Combine(GamePath, STARRAIL_EXE), Path.Combine(GamePath, $"{STARRAIL_EXE}.{suffix}.bak"));
            vm.ConvertingLog += $"aaaaaaaaaaaaaaaaaaaaaa \r\n";
        }


        /// <summary>
        /// 解压Pkg文件
        /// </summary>
        /// <param name="archiveName"></param>
        /// <returns></returns>
        private bool Decompress(string archiveName)
        {
            try
            {
                ZipFile.ExtractToDirectory($"{CurrentPath}/{archiveName}.pkg", CurrentPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 保存游戏设置
        /// </summary>
        /// <param name="vm"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SaveGameConfig(SettingsPageViewModel vm)
        {
            if (File.Exists(Path.Combine(App.Current.DataModel.GamePath, "config.ini")))
            {
                string bilibilisdk = "Plugins/PCGameSDK.dll";
                switch (vm.IsMihoyo)
                {
                    case 0:
                        App.Current.DataModel.Cps = "gw_PC";
                        App.Current.DataModel.Channel = 1;
                        App.Current.DataModel.Sub_channel = 1;
                        if (File.Exists(Path.Combine(GamePath, $"StarRail_Data/{bilibilisdk}")))
                            File.Delete(Path.Combine(GamePath, $"StarRail_Data/{bilibilisdk}"));
                        App.Current.NoticeOverAllBase.SwitchPort =
                            $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypePStr}";
                        App.Current.NoticeOverAllBase.IsGamePortLists = "Visible";
                        App.Current.NoticeOverAllBase.GamePortListIndex = 0;
                        break;
                    // todo bili
                    case 1:
                         App.Current.DataModel.Cps = "bilibili";
                         App.Current.DataModel.Channel = 14;
                         App.Current.DataModel.Sub_channel = 0;
                         if (!File.Exists(Path.Combine(GamePath, $"StarRail_Data/{bilibilisdk}")))
                         {
                             try
                             {
                                 string sdkPath = Path.Combine(GamePath, $"StarRail_Data/{bilibilisdk}");
                                 FileHelper.ExtractEmbededAppResource("StaticRes/PCGameSDK.dll", sdkPath);
                             }
                             catch (Exception ex)
                             {
                                 MessageBox.Show(ex.Message);
                             }
                         }
                         App.Current.NoticeOverAllBase.SwitchPort = $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypeBStr}";
                         App.Current.NoticeOverAllBase.IsGamePortLists = "Visible";
                         App.Current.NoticeOverAllBase.GamePortListIndex = 1;
                    
                         break;
                    case 2:
                        App.Current.DataModel.Cps = "hoyoverse_PC";
                        App.Current.DataModel.Channel = 1;
                        App.Current.DataModel.Sub_channel = 1;
                        if (File.Exists(Path.Combine(GamePath, $"GenshinImpact_Data/{bilibilisdk}")))
                            File.Delete(Path.Combine(GamePath, $"GenshinImpact_Data/{bilibilisdk}"));
                        App.Current.NoticeOverAllBase.SwitchPort =
                            $"{App.Current.Language.GameClientStr} : {App.Current.Language.GameClientTypeMStr}";
                        App.Current.NoticeOverAllBase.IsGamePortLists = "Hidden";
                        App.Current.NoticeOverAllBase.GamePortListIndex = -1;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}