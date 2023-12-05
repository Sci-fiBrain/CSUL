using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSUL.Models
{
    /// <summary>
    /// 界面语言管理类
    /// </summary>
    public class LanguageManager : IDisposable
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSUL_Lang.config");
        /// <summary>
        /// 可用语言字典，与语言选择下拉框绑定
        /// <para>Key:语言代码，添加新的语言是请将文件命名为正确的语言代码</para>
        /// <para>Value:语言名称，下拉框将显示此项</para>
        /// </summary>
        public static ReadOnlyDictionary<string, string> Languages = GetLanguages();

        public static LanguageManager Instance { get; } = new();

        #region ---构造函数---
        private LanguageManager()
        {
            bool loaded = false;
            if (File.Exists(ConfigPath))
            {
                try
                {
                    this.LoadConfig(ConfigPath);
                    loaded = true;
                }
                catch { }
            }
            if (!loaded)
            {
                CurrentLanguage = CultureInfo.CurrentCulture.Name.ToLower().Replace("-", "_");
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.SaveConfig(ConfigPath);
        }
        #endregion ---构造函数---

        #region ---私有字段---
        private string currentLanguage;
        #endregion ---私有字段---

        #region ---公共属性---
        [Config]
        public string CurrentLanguage
        {
            get => currentLanguage;
            set
            {
                if (value == currentLanguage) return;
                currentLanguage = value;
                SetLanguage(value);
            }
        }
        #endregion ---公共属性---

        #region ---私有方法---
        private void SetLanguage(string language)
        {
            if (Application.LoadComponent(new Uri($@"Languages\{language}.xaml", UriKind.Relative)) is ResourceDictionary rd)
            {
                ResourceDictionary resources = Application.Current.Resources.MergedDictionaries[0];
                foreach (object? key in rd.Keys)
                {
                    resources[key] = rd[key];
                }
            }
        }
        #endregion ---私有方法---

        #region ---静态方法---
        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public static ReadOnlyDictionary<string, string> GetLanguages()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string langDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");
            DirectoryInfo directory = new DirectoryInfo(langDir);
            FileInfo[] files = directory.GetFiles("*.xaml");
            foreach (FileInfo file in files)
            {
                string lang = Path.GetFileNameWithoutExtension(file.Name);
                lang = lang.ToLower();
                Trace.WriteLine(lang);
                try
                {
                    CultureInfo culture = CultureInfo.GetCultureInfo(lang.Replace("_", "-"));
                    dict.Add(lang, culture.NativeName);
                    Trace.WriteLine($"{culture.Name}: {culture.NativeName}");
                }
                catch { }
            }
            return new ReadOnlyDictionary<string, string>(dict);
        }
        #endregion ---静态方法---

    }
}
