using Microsoft.Win32;
using System.Text;
using StarRail_Launcher.Models;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using StarRail_Launcher.Service.IService;

namespace StarRail_Launcher.Service
{
    /// <summary>
    /// 对星铁注册表的操作
    /// </summary>
    public class RegistryService : IRegistryService
    {
        private const string CnPathKey = @"HKEY_CURRENT_USER\Software\miHoYo\崩坏：星穹铁道";
        private const string CnSdkKey = "MIHOYOSDK_ADL_PROD_CN_h3123967166";
        private const string GlobalPathKey = @"HKEY_CURRENT_USER\SOFTWARE\Cognosphere\Star Rail";
        private const string GlobalSdkKey = "MIHOYOSDK_ADL_PROD_OVERSEA_h1158948810";

        private const string PCResolutionKey = "GraphicsSettings_PCResolution_h431323223";
        private const string FullSizeKey = "Screenmanager Fullscreen mode_h3630240806";
        private const string HeightKey = "Screenmanager Resolution Height_h2627697771";
        private const string WidthKey = "Screenmanager Resolution Width_h182942802";
        
        // private const string DataKey = "GENERAL_DATA_h2389025596";

        /// <summary>
        /// 获取注册表内容
        /// </summary>
        /// <param name="name"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public string? GetFromRegistry(string name, string port, bool isSaveGameConfig)
        {
            RegistryModel userRegistry = new();
            userRegistry.Name = name;
            userRegistry.Port = port;
            try
            {
                if (port == "CN")
                {
                    object? cnsdk = Registry.GetValue(CnPathKey, CnSdkKey, string.Empty);
                    userRegistry.MIHOYOSDK_ADL_PROD = Encoding.UTF8.GetString((byte[])cnsdk);
                    // if (isSaveGameConfig)
                    // {
                    //     object? data = Registry.GetValue(CnPathKey, DataKey, string.Empty);
                    //     userRegistry.GENERAL_DATA = Encoding.UTF8.GetString((byte[])data);
                    // }
                }
                else if (port == "Global")
                {
                    object? globalsdk = Registry.GetValue(GlobalPathKey, GlobalSdkKey, string.Empty);
                    userRegistry.MIHOYOSDK_ADL_PROD = Encoding.UTF8.GetString((byte[])globalsdk);
                    // if (isSaveGameConfig)
                    // {
                    //     object? data = Registry.GetValue(GlobalPathKey, DataKey, string.Empty);
                    //     userRegistry.GENERAL_DATA = Encoding.UTF8.GetString((byte[])data);
                    // }
                }
            }
            catch
            {
                MessageBox.Show(App.Current.Language.SaveAccountErr);
            }
            return JsonConvert.SerializeObject(userRegistry);
        }

        /// <summary>
        /// 更新注册表内容
        /// </summary>
        /// <param name="name"></param>
        public void SetToRegistry(string name)
        {
            string file = Path.Combine(Directory.GetCurrentDirectory(), "UserData", name);
            string json = File.ReadAllText(file);
            RegistryModel userRegistry = JsonConvert.DeserializeObject<RegistryModel>(json);
            if (userRegistry.MIHOYOSDK_ADL_PROD != null &&
                userRegistry.MIHOYOSDK_ADL_PROD != "null" &&
                userRegistry.MIHOYOSDK_ADL_PROD != string.Empty)
            {
                // var fullSize = App.Current.DataModel.FullSize == 0 ? 3 : 1;
                // var isFullSize = fullSize == 3 ? false : true;
                if (userRegistry.Port == "CN")
                {
                    // JsonObject jsonObject = new();
                    // jsonObject.Add("width", int.Parse(App.Current.DataModel.Width));
                    // jsonObject.Add("height", int.Parse(App.Current.DataModel.Height));
                    // jsonObject.Add("isFullScreen", isFullSize);
                    // 
                    // Registry.SetValue(CnPathKey, PCResolutionKey, Encoding.UTF8.GetBytes(jsonObject.ToJsonString()));
                    // // 窗口、全屏
                    // Registry.SetValue(CnPathKey, FullSizeKey, fullSize, RegistryValueKind.DWord);
                    // Registry.SetValue(CnPathKey, "Screenmanager Resolution Height Default_h1380706816", App.Current.DataModel.Height, RegistryValueKind.DWord);
                    // Registry.SetValue(CnPathKey, "Screenmanager Resolution Width Default_h680557497", App.Current.DataModel.Height, RegistryValueKind.DWord);
                    // Registry.SetValue(CnPathKey, HeightKey, App.Current.DataModel.Height, RegistryValueKind.DWord);
                    // Registry.SetValue(CnPathKey, WidthKey, App.Current.DataModel.Width, RegistryValueKind.DWord);

                    Registry.SetValue(CnPathKey, CnSdkKey, Encoding.UTF8.GetBytes(userRegistry.MIHOYOSDK_ADL_PROD));
                    // if (userRegistry.GENERAL_DATA != null && userRegistry.GENERAL_DATA != "null" && userRegistry.GENERAL_DATA != string.Empty)
                    // {
                    //     Registry.SetValue(CnPathKey, DataKey, Encoding.UTF8.GetBytes(userRegistry.GENERAL_DATA));
                    // }
                }
                else if (userRegistry.Port == "Global")
                {
                    // JsonObject jsonObject = new();
                    // jsonObject.Add("width", App.Current.DataModel.Width);
                    // jsonObject.Add("height", App.Current.DataModel.Height);
                    // jsonObject.Add("isFullScreen", isFullSize);
                    // // 
                    // Registry.SetValue(GlobalPathKey, PCResolutionKey, Encoding.UTF8.GetBytes(jsonObject.ToJsonString()));
                    // 窗口、全屏
                    // Registry.SetValue(GlobalPathKey, FullSizeKey, fullSize, RegistryValueKind.DWord);
                    // Registry.SetValue(GlobalPathKey, HeightKey, App.Current.DataModel.Height, RegistryValueKind.DWord);
                    // Registry.SetValue(GlobalPathKey, WidthKey, App.Current.DataModel.Width, RegistryValueKind.DWord);

                    Registry.SetValue(GlobalPathKey, GlobalSdkKey,
                        Encoding.UTF8.GetBytes(userRegistry.MIHOYOSDK_ADL_PROD));
                    // if (userRegistry.GENERAL_DATA != null && userRegistry.GENERAL_DATA != "null" && userRegistry.GENERAL_DATA != string.Empty)
                    // {
                    //     Registry.SetValue(GlobalPathKey, DataKey, Encoding.UTF8.GetBytes(userRegistry.GENERAL_DATA));
                    // }
                }
                else
                {
                    MessageBox.Show("Error : The file does not support ! !");
                }
            }
            else
            {
                MessageBox.Show("Error : The file does not support ! !");
            }
        }
    }
}
