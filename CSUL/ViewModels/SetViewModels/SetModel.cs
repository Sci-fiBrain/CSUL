using CSUL.Models;
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
    }
}