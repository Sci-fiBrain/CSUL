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

        private void CButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }
    }
}