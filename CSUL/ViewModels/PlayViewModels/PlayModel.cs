using CSUL.Models;
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
            PlayGameCommand = new RelayCommand((sender) => GameManager.StartGame(FileManager.Instance.GamePath!));
        }

        public ICommand PlayGameCommand { get; }
    }
}