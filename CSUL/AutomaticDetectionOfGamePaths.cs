using System;
using Microsoft.Win32;
using System.IO;

namespace CSUL
{
    // <summary>
    /// 自动检测游戏路径
    /// </summary>
    class AutomaticDetectionOfGamePaths
    {
        //自动检测microsoftStore游戏路径
        public static string GetmicrosoftStoreGamePath()
        {
            string microsoftStoreGamePath = GetMicrosoftStoreGame();
            if (!string.IsNullOrEmpty(microsoftStoreGamePath))
                return microsoftStoreGamePath;
            else return null;
        }
        //自动检测Xbox游戏路径
        public static string GetXboxGamePath()
        {
            string xboxGamePath = GetXboxGame();
            if (!string.IsNullOrEmpty(xboxGamePath))
                return xboxGamePath;
            else return null;

        }
        private static string GetMicrosoftStoreGame()
        {
            // 微软商店游戏安装路径
            string defaultMicrosoftStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsApps");

            if (Directory.Exists(defaultMicrosoftStorePath))
            {
                // 获取WindowsApps目录下的第一个子目录（通常为游戏安装目录）
                string[] subdirectories = Directory.GetDirectories(defaultMicrosoftStorePath);
                if (subdirectories.Length > 0)
                {
                    return subdirectories[0];
                }
            }

            return null;
        }

        private static string GetXboxGame()
        {
            // Xbox游戏注册表路径
            const string xboxRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameUX\Games";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(xboxRegistryPath))
            {
                if (key != null)
                {
                    // 获取注册表中的游戏路径
                    string[] subKeyNames = key.GetSubKeyNames();
                    if (subKeyNames.Length > 0)
                    {
                        string firstGameKey = subKeyNames[0];
                        using (RegistryKey gameKey = key.OpenSubKey(firstGameKey))
                        {
                            if (gameKey != null)
                            {
                                return gameKey.GetValue("GameInstallLocation") as string;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}