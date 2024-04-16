using CSUL.Models;
using CSUL.Models.Local.GameEx;
using System;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.SetViewModels
{
    public class SetModel : BaseViewModel
    {
        private static ComParameters CP { get; } = ComParameters.Instance;

        public string GamePath
        {
            get => CP.GameRoot.FullName;
            set
            {
                if (value == GamePath) return;
                CP.GameRoot = new(value);
                OnPropertyChanged();
            }
        }

        public string GameData
        {
            get => CP.GameData.FullName;
            set
            {
                if (value == GameData) return;
                CP.GameData = new(value);
                OnPropertyChanged();
            }
        }

        public string? StartArgument
        {
            get => CP.StartArguemnt;
            set
            {
                if (value == StartArgument) return;
                CP.StartArguemnt = value;
                OnPropertyChanged();
            }
        }

        public string? SteamPath
        {
            get => CP.SteamPath;
            set
            {
                if (value == SteamPath) return;
                CP.SteamPath = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenWebUri { get; } = new RelayCommand(sender =>
            System.Diagnostics.Process.Start("explorer.exe", sender as string ?? ""));

        public ICommand LogoutParadox { get; } = new RelayCommand(sender =>
        {
            try
            {
                var ret = MessageBox.Show("这将会删除已登录的Paradox账号数据", "确认登出Paradox账号吗？", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (ret != MessageBoxResult.OK) return;
                GameDataManager.LogoutParadox();
                MessageBox.Show("Paradox账号已退出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToFormative(), "退出Paradox账号时出现问题", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        });
    }
}