using CSUL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.ModViewModels
{   //ModModel用到的属性、字段
    public partial class ModModel
    {
        /// <summary>
        /// 临时文件夹路径
        /// </summary>
        private static readonly string _tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempFile");

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

        private List<ItemData> modData = default!;

        public List<ItemData> ModData
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

        //添加新插件
        public ICommand AddCommand { get; }

        //删除插件
        public ICommand DeleteCommand { get; }

        //下载BepInEx
        public ICommand DownloadCommand { get; }

        //检查mod的版本兼容性
        public ICommand CheckMods { get; }

        //打开文件夹
        public ICommand OpenFolder { get; } = new RelayCommand((sender)
            => Process.Start("Explorer.exe", FileManager.Instance.ModDir.FullName));

        //移除BepInEx
        public ICommand RemoveCommand { get; }

        //刷新
        public ICommand Refresh { get; }

        #endregion ---Commands---
    }
}