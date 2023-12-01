using CSUL.ViewModels.PlayViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CSUL.Views
{
    /// <summary>
    /// PlayView.xaml 的交互逻辑
    /// </summary>
    public partial class PlayView : UserControl
    {
        public PlayView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {   //加载完成后
            (DataContext as PlayModel)?.SetWindow(Window.GetWindow(this));
        }
    }
}