/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: Chinesization.cs
 *  创建时间: 2023年12月24日 1:13
 *  创建开发: ScifiBrain
 *  文件介绍: 汉化游戏资源
 *  --------------------------------------
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace CSUL.Models.Local.GameEx
{
    /// <summary>
    /// 汉化游戏资源类
    /// </summary>
    internal static class Chinesization
    {
        private const string jsPath = "Cities2_Data\\StreamingAssets\\~UI~\\ModsUI\\main.js";
        private const string startString = "///////////REPLACE_START///////////";
        private const string endString = "//REPLACE_ITEMS_END//";

        /// <summary>
        /// 汉化游戏(添加汉化数据)
        /// </summary>
        /// <param name="cnText">汉化内容</param>
        public static void Chinesize(string cnText)
        {
            string path = Path.Combine(ComParameters.Instance.GameRoot.FullName, jsPath);
            if (!File.Exists(path)) return;
            string data = File.ReadAllText(path);
            int startIndex = data.IndexOf('{') + 1;
            if (startIndex == 0) throw new Exception("错误的 main.js 文件");
            data = data.Insert(startIndex, $"{Environment.NewLine}{cnText}{Environment.NewLine}");
            File.WriteAllText(path, data, Encoding.UTF8);
        }

        /// <summary>
        /// 移除旧的汉化数据
        /// </summary>
        public static void RemoveOutdate()
        {
            string path = Path.Combine(ComParameters.Instance.GameRoot.FullName, jsPath);
            if (!File.Exists(path)) return;
            string data = File.ReadAllText(path, Encoding.UTF8);
            data.ReplaceLineEndings();
            int left = data.IndexOf(startString);
            int right = data.LastIndexOf(endString) + endString.Length;
            if (left != -1 && right != -1) data = data.Remove(left, right - left);
            data = data.Replace(Environment.NewLine, null);
            File.WriteAllText(path, data, Encoding.UTF8);
        }

        /// <summary>
        /// 是否已安装汉化数据
        /// </summary>
        public static bool IsInstalled()
        {
            string path = Path.Combine(ComParameters.Instance.GameRoot.FullName, jsPath);
            if (!File.Exists(path)) return false;
            string data = File.ReadAllText(path, Encoding.UTF8);
            if (data.Contains(startString)) return true;
            else return false;
        }

        /// <summary>
        /// 获取已安装的汉化版本
        /// </summary>
        public static bool TryGetInstalledVersion([NotNullWhen(true)] out Version? version)
        {
            version = null;
            string path = Path.Combine(ComParameters.Instance.GameRoot.FullName, jsPath);
            if (!File.Exists(path)) return false;
            string data = File.ReadAllText(path, Encoding.UTF8);
            version = GetVersion(data);
            return version != null;
        }

        /// <summary>
        /// 获取汉化数据的版本
        /// </summary>
        /// <param name="cnText">汉化数据</param>
        /// <returns>版本</returns>
        public static Version? GetVersion(string cnText)
        {
            const string tag = "//v ";
            int startIndex = cnText.IndexOf(tag);
            if (startIndex == -1) return null;
            string data = "";
            for (int i = startIndex + tag.Length; i < startIndex + tag.Length + 30; i++)
            {
                if (cnText[i] == '\n')
                {
                    if (Version.TryParse(data.Trim(), out Version? version)) return version;
                    else return null;
                }
                data += cnText[i];
            }
            return null;
        }
    }
}