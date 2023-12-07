using CSUL.Models;
using CSUL.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSUL.ViewModels
{
    /// <summary>
    /// MainWindow的ViewModel
    /// </summary>
    internal class MainModel : BaseViewModel
    {
        public MainModel()
        {
            ViewContents = new()
            {   //View列表
                new PlayView(),
                new MapView(),
                new SaveView(),
                new ModView(),
                new SetView()
            };
            viewContent = ViewContents[0];
            ViewCommand = new RelayCommand((x) =>
            {
                //判断传参x是否符合条件
                if (x is not string text || !int.TryParse(text, out int num)) throw new ArgumentNullException(nameof(x));
                ChangeViewContent(num);
            });
            Task.Run(CheckVersion);
        }

        #region ---公共属性---

        private UserControl viewContent;

        /// <summary>
        /// 当前显示的页面
        /// </summary>
        public UserControl ViewContent
        {
            get => viewContent;
            set
            {
                viewContent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 页面列表
        /// </summary>
        public List<UserControl> ViewContents { get; }

        public ICommand ViewCommand { get; }

        #endregion ---公共属性---

        #region ---私有方法---

        /// <summary>
        /// 改变当前显示的页面
        /// </summary>
        /// <param name="index">目标页面索引</param>
        private void ChangeViewContent(int index)
        {
            ViewContent = ViewContents[index];
        }

        /// <summary>
        /// 检查CSUL版本
        /// </summary>
        /// <returns></returns>
        private async Task CheckVersion()
        {
            Version? latest = await WebManager.GetLatestCsulVersion();
            if (latest is null) return;
            Version? version = CsulVersion;
            if (version is null) return;
            if (version > latest)
            {
                StringBuilder builder = new();
                builder.Append("CSUL有新版本更新").AppendLine();
                builder.Append("当前版本: ").Append(version).AppendLine();
                builder.Append("最新版本: ").Append(latest).AppendLine();
                builder.Append("点击确认跳转到CSUL发布页面");
                MessageBoxResult ret = MessageBox.Show(builder.ToString(), "更新提示", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (ret == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "https://www.cslbbs.net/csul/");
                }
            }
        }

        #endregion ---私有方法---
    }
}