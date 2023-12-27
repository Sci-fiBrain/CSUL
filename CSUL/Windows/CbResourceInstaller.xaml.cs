/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: CbResourceInstaller.xaml.cs
 *  创建时间: 2023年12月28日 0:14
 *  创建开发: 
 *  文件介绍: 
 *  --------------------------------------
 */
using CSUL.Models;
using CSUL.Models.Local.ModPlayer;
using CSUL.Models.Network.CB;
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
    /// CbResourceInstaller.xaml 的交互逻辑
    /// </summary>
    public partial class CbResourceInstaller : Window
    {
        private static readonly ComParameters CP = ComParameters.Instance;
        private CbResourceInstaller() { }

        internal CbResourceInstaller(List<CbResourceData> datas)
        {
            InitializeComponent();

            //播放集
            playerCombo.ItemsSource = CP.ModPlayerManager.GetModPlayers().Select(x => x.PlayerName);
            playerCombo.SelectedIndex = 0;

            //模组列表
            modList.ItemsSource = datas;
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

        private void CButton_Click(object sender, RoutedEventArgs e)
        {
            BaseModPlayer? player = playerCombo.SelectedItem is not string name ? null : CP.ModPlayerManager.GetModPlayers().FirstOrDefault(x => x.PlayerName == name);
            if (player is null or NullModPlayer)
            {
                MessageBox.Show("还没有选择播放集\n请选择或在模组管理页创建一个播放集", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (modList.ItemsSource is not List<CbResourceData> datas)
            {
                MessageBox.Show("获取模组列表失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
