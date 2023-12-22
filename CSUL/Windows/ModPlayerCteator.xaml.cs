using CSUL.ViewModels.ModPlayerCreatorViewModels;
using System.Windows;
using System.Windows.Input;

namespace CSUL.Windows
{
    /// <summary>
    /// ModPlayerCteator.xaml 的交互逻辑
    /// </summary>
    public partial class ModPlayerCteator : Window
    {
        internal ModPlayerCteator()
        {
            InitializeComponent();
            ModPlayerCreatorModel? vm = DataContext as ModPlayerCreatorModel;
            if (vm is not null) vm.Window = this;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}