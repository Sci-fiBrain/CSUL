using CSUL.Models;
using CSUL.Models.Local.ModPlayer.BepInEx;
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
        private static ComParameters CP { get; } = ComParameters.Instance;

        /// <summary>
        /// 临时文件夹路径
        /// </summary>
        private static readonly string _tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempFile");

        #region ---显示“未安装BepInEx”页面---

        private Visibility showNoEx = BepManager.IsInstalled(CP.GameRoot.FullName) ? Visibility.Visible : Visibility.Collapsed;

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

        public Version? BepVersion
        {
            get => BepManager.TryGetBepVersion(CP.GameRoot.FullName, out Version? ver) ? ver : null;
            set => OnPropertyChanged();
        }

        #endregion ---BepInEx的版本---

        #region ---BepInEx的下载数据---

        private List<BepItemData>? bepData = BepManager.IsInstalled(CP.GameRoot.FullName) ? GetBepDownloadData() : null;

        public List<BepItemData>? BepData
        {
            get => bepData;
            private set
            {
                if (bepData == value) return;
                bepData = value;
                OnPropertyChanged();
            }
        }

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
            => Process.Start("Explorer.exe", CP.BepMod.FullName));

        //移除BepInEx
        public ICommand RemoveCommand { get; }

        //刷新
        public ICommand Refresh { get; }

        #endregion ---Commands---
    }
}