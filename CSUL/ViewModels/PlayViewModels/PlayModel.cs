using CSUL.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace CSUL.ViewModels.PlayViewModels
{
    /// <summary>
    /// PlayView的ViewModel
    /// </summary>
    public class PlayModel : BaseViewModel
    {
        public PlayModel()
        {
            PlayGameCommand = new RelayCommand(
                (sender) =>
                {
                    try
                    {
                        string arg = $"{(OpenDeveloper ? "-developerMode " : null)} {GameManager.Instance.StartArguemnt}";
                        GameManager.StartGame(FileManager.Instance.GamePath!, arg);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ExceptionManager.GetExMeg(ex), "游戏启动出现错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
        }

        public ICommand PlayGameCommand { get; }

        public bool OpenDeveloper
        {
            get => GameManager.Instance.OpenDeveloper;
            set
            {
                GameManager.Instance.OpenDeveloper = value;
                OnPropertyChanged();
            }
        }
    }
}