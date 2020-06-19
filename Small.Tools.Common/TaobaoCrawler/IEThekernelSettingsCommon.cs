using Microsoft.Win32;
using System;
using System.ComponentModel;

namespace Small.Tools.Common.TaobaoCrawler
{
    /// <summary>
    /// IE内核设置
    /// </summary>
    public class IEThekernelSettingsCommon
    {

        /* int ieVersion = IEThekernelSettingsCommon.GetBrowserVersion();
        if (IEThekernelSettingsCommon.IfWindowsSupport())
        {
            IEThekernelSettingsCommon.SetWebBrowserFeatures(ieVersion< 11 ? ieVersion : 11);
        }
        else
        {
            //如果不支持IE8  则修改为当前系统的IE版本
            IEThekernelSettingsCommon.SetWebBrowserFeatures(ieVersion< 7 ? 7 : ieVersion);
        }

        IEThekernelSettingsCommon.SetWebBrowserFeatures(11);
        */

        /// <summary>  
        /// 修改注册表信息来兼容当前程序
        /// IE 11,默认: 11
        /// </summary>  
        public static void SetWebBrowserFeatures(int ieVersion = 11)
        {
            // don't change the registry if running in-proc inside Visual Studio  
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime) return;

            //获取程序及名称
            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            //得到浏览器的模式的值  
            UInt32 ieMode = GeoEmulationModee(ieVersion);
            var featureControlRegKey = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\";

            //设置浏览器对应用程序（appName）以什么模式（ieMode）运行  
            Registry.SetValue(featureControlRegKey + "FEATURE_BROWSER_EMULATION",
                appName, ieMode, RegistryValueKind.DWord);
            // enable the features which are "On" for the full Internet Explorer browser  

            Registry.SetValue(featureControlRegKey + "FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", appName, 1, RegistryValueKind.DWord);

        }

        /// <summary>
        /// 获取“IE”浏览器的版本
        /// </summary>
        /// <returns>int</returns>  
        public static int GetBrowserVersion()
        {
            int browserVersion = 0;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            if (browserVersion < 7)
                throw new ApplicationException("不支持的浏览器版本，请自行将“IE浏览器升级到11” 。");

            return browserVersion;
        }

        /// <summary>
        /// 查询系统环境是否支持IE8以上版本
        /// </summary>
        public static bool IfWindowsSupport()
        {
            bool isWin7 = Environment.OSVersion.Version.Major > 6;
            bool isSever2008R2 = Environment.OSVersion.Version.Major == 6
                && Environment.OSVersion.Version.Minor >= 1;

            if (!isWin7 && !isSever2008R2)
            {
                return false;
            }
            else return true;
        }

        /// <summary>  
        /// 通过版本得到浏览器模式的值
        /// </summary>  
        /// <param name="browserVersion"></param>  
        /// <returns>UInt32</returns>  
        private static UInt32 GeoEmulationModee(int browserVersion)
        {
            UInt32 mode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode.   
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode.   
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode.   
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode.                      
                    break;
                case 10:
                    mode = 10000; // Internet Explorer 10.  
                    break;
                case 11:
                    mode = 11000; // Internet Explorer 11  
                    break;
            }
            return mode;
        }
    }
}
