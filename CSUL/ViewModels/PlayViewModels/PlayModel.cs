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
                        GameManager.StartGame(FileManager.Instance.GamePath!);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ExceptionManager.GetExMeg(ex), "游戏启动出现错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
        }

        public ICommand PlayGameCommand { get; }
    }
}