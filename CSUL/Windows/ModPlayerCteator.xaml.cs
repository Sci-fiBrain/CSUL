using CSUL.Models.Local.ModPlayer;
using CSUL.ViewModels.ModPlayerCreatorViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
