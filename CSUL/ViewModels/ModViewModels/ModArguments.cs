using CSUL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace CSUL.ViewModels.ModViewModels
{   //ModModel用到的属性、字段
    public partial class ModModel
    {
        #region ---显示“未安装BepInEx”页面---

        private Visibility showNoEx = FileManager.Instance.NoBepInEx ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ShowNoEx
        {
            get => showNoEx;
            set
            {
                if (showNoEx == value) return;
                showNoEx = value;
                OnPropertyChanged();
            }
        }

        #endregion ---显示“未安装BepInEx”页面---

        #region ---BepInEx的版本---

        private Version? bepVersion = FileManager.Instance.BepVersion;

        public Version? BepVersion
        {
            get => bepVersion;
            set
            {
                if (bepVersion == value) return;
                bepVersion = value;
                OnPropertyChanged();
            }
        }

        #endregion ---BepInEx的版本---

        #region ---BepInEx的下载数据---

        public List<BepItemData>? BepData { get; private set; } = FileManager.Instance.NoBepInEx ? GetBepDownloadData() : null;

        #endregion ---BepInEx的下载数据---

        #region ---模组信息列表---

        private List<ModInfo> modData;

        public List<ModInfo> ModData
        {
            get => modData;
            set
            {
                if (modData == value) return;
                modData = value;
                OnPropertyChanged();
            }
        }

        #endregion ---模组信息列表---

        #region ---Commands---

        /// <summary>
        /// 添加新插件
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// 删除插件
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// 下载BepInEx
        /// </summary>
        public ICommand DownloadCommand { get; }

        /// <summary>
        /// 检查mod的版本兼容性
        /// </summary>
        public ICommand CheckMods { get; }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        public ICommand OpenFolder { get; } = new RelayCommand((sender)
            => Process.Start("Explorer.exe", FileManager.Instance.ModDir.FullName));

        /// <summary>
        /// 刷新mod列表
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// 移除BepInEx
        /// </summary>
        public ICommand RemoveCommand { get; }

        /// <summary>
        /// 启用mod命令
        /// </summary>
        public ICommand EnableCommand { get; }

        /// <summary>
        /// 禁用mod命令
        /// </summary>
        public ICommand DisableCommand { get; }

        #endregion ---Commands---
    }
}