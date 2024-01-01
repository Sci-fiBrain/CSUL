/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: Chinesization.cs
 *  创建时间: 2023年12月24日 1:13
 *  创建开发: ScifiBrain
 *  文件介绍: 汉化游戏资源
 *  --------------------------------------
 */

using System;
using System.IO;
using System.Text;

namespace CSUL.Models.Local.GameEx
{
    /// <summary>
    /// 汉化游戏资源类
    /// </summary>
    internal static class Chinesization
    {
        /// <summary>
        /// 汉化游戏(添加汉化数据)
        /// </summary>
        /// <param name="rootPath">游戏根目录</param>
        /// <param name="cnText">汉化内容</param>
        public static void Chinesize(string rootPath, string cnText)
        {
            string path = Path.Combine(rootPath, "Cities2_Data\\StreamingAssets\\~UI~\\GameUI\\index.js");
            if (!File.Exists(path)) throw new FileNotFoundException("未找到 index.js (游戏)文件\n请检查游戏安装路径设置");
            string data = File.ReadAllText(path);
            int startIndex = data.IndexOf('{') + 1;
            if (startIndex == 0) throw new Exception("错误的 index.js 文件");
            data = data.Insert(startIndex, $"{Environment.NewLine}{cnText}{Environment.NewLine}");
            File.WriteAllText(path, data, Encoding.UTF8);
        }

        /// <summary>
        /// 移除旧的汉化数据
        /// </summary>
        /// <param name="rootPath">游戏根目录</param>
        /// <returns></returns>
        public static void RemoveOutdate(string rootPath)
        {
            string path = Path.Combine(rootPath, "Cities2_Data\\StreamingAssets\\~UI~\\GameUI\\index.js");
            if (!File.Exists(path)) throw new FileNotFoundException("未找到 index.js (游戏)文件\n请检查游戏安装路径设置");
            string data = File.ReadAllText(path, Encoding.UTF8);
            data.ReplaceLineEndings();
            const string startString = "//REPLACE_ITEMS_START//";
            const string endString = "//REPLACE_ITEMS_END//";
            int left = data.IndexOf(startString);
            int right = data.LastIndexOf(endString) + endString.Length;
            if (left != -1 && right != -1) data = data.Remove(left, right - left);
            data = data.Replace(Environment.NewLine, null);
            File.WriteAllText(path, data, Encoding.UTF8);
        }

        /// <summary>
        /// 获取汉化数据的版本
        /// </summary>
        /// <param name="cnText">汉化数据</param>
        /// <returns>版本</returns>
        public static Version? GetVersion(string cnText)
        {
            const string tag = "//version=";
            int startIndex = cnText.IndexOf(tag);
            if (startIndex == -1) return null;
            string data = "";
            for (int i = startIndex + tag.Length; i < startIndex + tag.Length + 30; i++)
            {
                if (cnText[i] == '/')
                {
                    if (Version.TryParse(data, out Version? version)) return version;
                    else return null;
                }
                data += cnText[i];
            }
            return null;
        }
    }
}