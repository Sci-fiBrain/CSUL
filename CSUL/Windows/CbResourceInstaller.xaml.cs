using CSUL.Models;
using CSUL.Models.Local;
using CSUL.Models.Local.ModPlayer;
using CSUL.Models.Local.ModPlayer.BepInEx;
using CSUL.Models.Local.ModPlayer.Pmod;
using CSUL.Models.Network;
using CSUL.Models.Network.CB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CSUL.Windows
{
    /// <summary>
    /// CbResourceInstaller.xaml 的交互逻辑
    /// </summary>
    public partial class CbResourceInstaller : Window
    {
        private static readonly ComParameters CP = ComParameters.Instance;

        private CbResourceInstaller()
        {
        }

        internal CbResourceInstaller(List<CbResourceData> datas)
        {
            InitializeComponent();

            //播放集
            playerCombo.ItemsSource = CP.ModPlayerManager.GetModPlayers().Select(x => x.PlayerName);
            playerCombo.SelectedItem = (playerCombo.ItemsSource as IEnumerable<string>)?.FirstOrDefault(x => x == CP.SelectedModPlayer)
                ?? (playerCombo.ItemsSource as IEnumerable<string>)?.FirstOrDefault();

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

        private async void CButton_Click(object sender, RoutedEventArgs e)
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
            try
            {
                IsEnabled = false;
                Topmost = false;
                if (player is BepModPlayer)
                {
                    Version? bepVersion = player.PlayerVersion ?? throw new Exception("获取播放集的BepInEx版本信息失败");
                    foreach (CbResourceData data in datas)
                    {
                        if (data.CustomInfo is not CbCustomBepModInfo customInfo) continue;
                        try
                        {
                            BepModData modData = new(string.Empty)
                            {   //得到模组信息
                                Id = data.Id,
                                ModVersion = data.ResourceVersion,
                                Description = data.Desciption,
                                ModUrl = data.ResourceUrl,
                                Name = data.Title
                            };
                            CbFileData fileData;
                            if (customInfo.BepInExVersion?.Length > 1)
                            {
                                string pattern = "(b(e?|(ep)?)((in)?(ex)?)?|ex)-?" + bepVersion.Major;
                                fileData = (data.Files?.FirstOrDefault(x => Regex.IsMatch(x.FileName, pattern, RegexOptions.IgnoreCase)))
                                    ?? throw new Exception("获取模组文件下载地址失败");
                            }
                            else
                            {
                                fileData = data.Files?.FirstOrDefault() ?? throw new Exception("获取模组文件下载地址失败");
                            }

                            using TempDirectory temp = new();
                            async Task<string> DownloadFile()
                            {   //下载资源的内敛方法
                                string path = Path.Combine(temp.FullName, fileData.FileName);
                                using Stream stream = File.Create(path);
                                await NetworkData.DownloadFromUri(fileData.Url, stream, api: true);
                                return path;
                            }

                            //查找模组是否已存在
                            BepModData? oldModData = player.FirstOrDefault(x =>
                            {
                                if (x is not BepModData bepModData) return false;
                                if (bepModData.Id == modData.Id) return true;
                                return bepModData.Name == modData.Name;
                            }) as BepModData;
                            if (oldModData is not null)
                            {
                                MessageBoxResult ret = MessageBox.Show($"模组[{modData.Id}] {modData.Name}已存在\n" +
                                    $"现存版本: {oldModData.ModVersion}\n" +
                                    $"新装版本: {modData.ModVersion}\n" +
                                    $"是否覆盖原有模组?", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                                if (ret == MessageBoxResult.Cancel) continue;
                                await player.UpgradeMod(oldModData, await DownloadFile(), modData);
                            }
                            else
                            {
                                await player.AddMod(await DownloadFile(), modData);
                            }
                            MessageBox.Show($"模组{data.Title}自动安装结束", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToFormative(data.Title), "模组安装失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else if (player is PmodPlayer)
                {
                    foreach (CbResourceData data in datas)
                    {
                    }
                }
                else throw new Exception("不支持的播放集类型" + player.GetType().Name);
                MessageBox.Show("所有安装任务已完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToFormative(), "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                IsEnabled = true;
            }
        }
    }
}