using CSUL.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CSUL.Models
{
    public class PluginInfo : BaseViewModel
    {
        #region 私有字段
        private string name;
        private string version;
        private string mainFile = "";
        private bool isMainFileFound = false;
        private bool isEnabled = false;
        private ICommand enableCommand;
        private ICommand disableCommand;
        #endregion

        #region 构造函数
        public PluginInfo(string name, string version)
        {
            Name = name;
            Version = version;
        }
        #endregion

        #region 属性
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Version
        {
            get => version;
            set
            {
                version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
        public string MainFile
        {
            get => mainFile;
            private set
            {
                mainFile = value;
                OnPropertyChanged(nameof(MainFile));
            }
        }
        public bool IsMainFileFound
        {
            get => isMainFileFound;
            private set
            {
                isMainFileFound = value;
                OnPropertyChanged(nameof(IsMainFileFound));
            }
        }
        public bool IsEnabled
        {
            get => isEnabled;
            private set
            {
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        #endregion

        #region 公有方法
        public void SetMainFile(string mainFile)
        {
            if (Path.Exists(mainFile) && (mainFile.ToLower().EndsWith(".dll") || mainFile.ToLower().EndsWith(".dlloff")))
            {
                MainFile = mainFile;
                IsMainFileFound = true;
                IsEnabled = mainFile.ToLower().EndsWith(".dll");
            }
        }

        public void Disable(object? sender)
        {
            Trace.WriteLine("Disable");
            if (!IsEnabled) { return; }
            FileInfo file = new FileInfo(MainFile);
            if (file.Exists && MainFile.ToLower().EndsWith(".dll"))
            {
                Trace.WriteLine("True");
                string newFileName = MainFile + "off";
                file.MoveTo(newFileName);
                IsEnabled = false;
                MainFile = newFileName;
            }
        }

        public void Enable(object? sender)
        {
            Trace.WriteLine("Enable");
            if (IsEnabled) { return; }
            FileInfo file = new FileInfo(MainFile);
            if (file.Exists/* && MainFile.ToLower().EndsWith(".dlloff")*/)
            {
                Trace.WriteLine("True");
                string newFileName = MainFile.Replace("dlloff", "dll");
                file.MoveTo(newFileName);
                IsEnabled = true;
                MainFile = newFileName;
            }
        }
        #endregion

        #region Command
        public ICommand EnableCommand
        {
            get
            {
                if (enableCommand == null)
                {
                    enableCommand = new RelayCommand(Enable);
                }
                return enableCommand;
            }
        }
        public ICommand DisableCommand
        {
            get
            {
                if (disableCommand == null)
                {
                    disableCommand = new RelayCommand(Disable);
                }
                return disableCommand;
            }
        }
        #endregion


    }
}
