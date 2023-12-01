using CSUL.Models;
using System.Windows.Input;

namespace CSUL.ViewModels.SetViewModels
{
    public class SetModel : BaseViewModel
    {
        public string GamePath
        {
            get => FileManager.Instance.GameRootDir.FullName!;
            set
            {
                if (value == FileManager.Instance.GamePath) return;
                FileManager.Instance.GameRootDir = new(value);
                OnPropertyChanged();
            }
        }

        public string GameData
        {
            get => FileManager.Instance.GameDataDir.FullName;
            set
            {
                if (value == FileManager.Instance.GameDataDir.FullName) return;
                FileManager.Instance.GameDataDir = new(value);
                OnPropertyChanged();
            }
        }

        public string? StartArgument
        {
            get => GameManager.Instance.StartArguemnt;
            set
            {
                GameManager.Instance.StartArguemnt = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenWebUri { get; } = new RelayCommand(sender =>
            System.Diagnostics.Process.Start("explorer.exe", sender as string ?? ""));
    }
}