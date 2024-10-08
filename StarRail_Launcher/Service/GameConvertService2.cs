﻿using StarRail_Launcher.Service.IService;
using StarRail_Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using StarRail_Launcher.Helper;
using StarRail_Launcher.Core;
using StarRail_Launcher.Models;

namespace StarRail_Launcher.Service
{
    internal class ConvertService : IGameConvertService
    {
        private const string CN_DIRECTORY = "CnFile";

        private const string GLOBAL_DIRECTORY = "GlobalFile";

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

        public string ConfigValue(String code)
        {
            TextReader iniFile = null;
            string strLine = null;
            string currentRoot = null;
            string iniFilePath = Path.Combine(GamePath ?? "", "Config.ini");
            if (File.Exists(iniFilePath))
            {
                try
                {
                    iniFile = new StreamReader(iniFilePath);
                    strLine = iniFile.ReadLine();
                    while (strLine != null)
                    {
                        strLine = strLine.Trim();
                        if (strLine != string.Empty)
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                string[] keyPair = strLine.Split(new char[] { '=' }, 2);
                                if (keyPair[0] == code)
                                {
                                    return keyPair.Length > 1 ? keyPair[1] : null;
                                }
                            }
                        }
                        strLine = iniFile.ReadLine();
                    }
                }
                catch { }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                }
            }
            else
            {
                if (!Directory.Exists(@"Config"))
                {
                    Directory.CreateDirectory("Config");
                }
            }
            return null;
        }

        public async Task<int> CheckPackageVersionAsync(string scheme, SettingsPageViewModel vm)
        {
            if (App.Current.PkgUpdataModel.PkgVersion == "" || App.Current.PkgUpdataModel.PkgVersion == null || App.Current.PkgUpdataModel.PkgVersion == string.Empty)
            {
                for (int i = 0; i < 10; i++)
                {
                    vm.ConvertingLog = $"获取PKG版本号失败，尝试重新获取{i + 1}";
                    if (App.Current?.PkgUpdataModel != null)
                    {
                        App.Current.PkgUpdataModel.PkgVersion = await HtmlHelper.GetPkgVersionAsync();
                    }
                    else
                    {
                        // 初始化 PkgUpdataModel 或处理 null 情况
                        App.Current.PkgUpdataModel = new PkgUpdataModel();
                        App.Current.PkgUpdataModel.PkgVersion = await HtmlHelper.GetPkgVersionAsync();
                    }
                    if (App.Current.PkgUpdataModel.PkgVersion != "" || App.Current.PkgUpdataModel.PkgVersion != null || App.Current.PkgUpdataModel.PkgVersion != string.Empty)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }
                if (App.Current.PkgUpdataModel.PkgVersion == "" || App.Current.PkgUpdataModel.PkgVersion == null || App.Current.PkgUpdataModel.PkgVersion == string.Empty)
                {
                    vm.ConvertingLog = $"获取PKG版本号失败，请检查你的网络设置。";
                    return 1;
                }
            }
            string gameversion = ConfigValue("game_version");
            string pkgfile = App.Current.PkgUpdataModel.PkgVersion + "-1";
            if (gameversion != App.Current.PkgUpdataModel.PkgVersion)
            {
                vm.ConvertingLog = $"当前游戏版本过低，请前往米哈游启动器更新游戏。\r\n当前游戏版本号：{gameversion}\r\n当前从API获取的游戏版本号：{App.Current.PkgUpdataModel.PkgVersion}\r\n";
                return 1;
            }
            if (!File.Exists($"{scheme}/{pkgfile}"))
            {
                vm.ConvertingLog = $"{App.Current.Language.NewPkgVer} : [{pkgfile}]\r\n即将下载最新版本转换包。\r\n请将下载好的转换包移动至本软件软件目录下。";
                await Task.Delay(1000);
                if (scheme == CN_DIRECTORY)
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = "https://download.ganyu.us.kg/now/StarRail/CnFile.pkg",
                        UseShellExecute = true,
                    };
                    Process.Start(info);
                }
                else
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = "https://download.ganyu.us.kg/now/StarRail/GlobalFile.pkg",
                        UseShellExecute = true,
                    };
                    Process.Start(info);
                }
                return 0;
            }
            else
            {
                return 2;
            }
        }

        public async Task GetFilesNameFromJson(string jsonFilePath)
        {
            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string json = await File.ReadAllTextAsync(jsonFilePath);
                    List<string> gameFileList = JsonSerializer.Deserialize<List<string>>(json);

                    foreach (string file in gameFileList)
                    {
                        // 处理文件路径
                        string temp = file.Replace(Path.Combine(Environment.CurrentDirectory, ReplaceSourceDirectory), "");
                        GameFileList.Add(temp);
                    }

                    // 删除 JSON 文件
                    File.Delete(jsonFilePath);
                }
                else
                {
                    MessageBox.Show("GameFileList.json 文件不存在");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                    string jsonFilePath = Path.Combine(CurrentPath, "Config", "GameFileList.json");
                    if (File.Exists(jsonFilePath))
                    {
                        await GetFilesNameFromJson(jsonFilePath);
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
                    int up = await CheckPackageVersionAsync(ReplaceSourceDirectory, vm);
                    switch (up)
                    {
                        case 1:
                            {
                                vm.ConvertState = false;
                                break;
                            }

                        case 2:
                            {
                                vm.StateIndicator = "正在获取文件清单";
                                await GetFilesName(Path.Combine(CurrentPath, ReplaceSourceDirectory));
                                await ReplaceGameFiles(vm);
                                break;
                            }

                        default:
                            {
                                Directory.Delete($"{CurrentPath}/{ReplaceSourceDirectory}", true);
                                vm.ConvertState = false;
                                break;
                            }
                    }
                    return;
                }
                else if (File.Exists(Path.Combine(CurrentPath, $"{PkgPerfix}.pkg")))
                {
                    vm.StateIndicator = "开始解压Pkg文件";
                    //解压 pkg 文件
                    if (Decompress(PkgPerfix))
                    {
                        int up = await CheckPackageVersionAsync(ReplaceSourceDirectory, vm);
                        switch (up)
                        {
                            case 1:
                                {
                                    vm.ConvertState = false;
                                    break;
                                }

                            case 2:
                                {
                                    //直接从 pkg解压后的目录 处替换
                                    vm.StateIndicator = "正在获取文件清单";
                                    await GetFilesName(Path.Combine(CurrentPath, ReplaceSourceDirectory));
                                    await ReplaceGameFiles(vm);
                                    break;
                                }

                            default:
                                {
                                    Directory.Delete($"{CurrentPath}/{ReplaceSourceDirectory}", true);
                                    vm.ConvertState = false;
                                    break;
                                }
                        }
                        return;
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
                    string temp = file.FullName.Replace(Path.Combine(Environment.CurrentDirectory, ReplaceSourceDirectory), "");
                    GameFileList.Add(temp);
                }
                DirectoryInfo[] dirs = directoryInfo.GetDirectories();
                if (dirs.Length > 0)
                {
                    foreach (DirectoryInfo dir in dirs)
                    {
                        await GetFilesName(dir.FullName);
                    }
                }

                // 将 GameFileList 序列化为 JSON 并保存到 Config 文件夹中
                string json = JsonSerializer.Serialize(GameFileList);
                string configDirectory = Path.Combine(Environment.CurrentDirectory, "Config");
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                }
                string filePath = Path.Combine(configDirectory, "GameFileList.json");
                await File.WriteAllTextAsync(filePath, json);
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
        /// 

        


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
            switch (ConfigValue("cps"))
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
            string suffix = GetCurrentSchemeName() == GLOBAL_DIRECTORY ? "cn" : "global";

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
            string suffix = GetCurrentSchemeName() == GLOBAL_DIRECTORY ? "global" : "cn";

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