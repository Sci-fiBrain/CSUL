/*  CSUL 标准文件头注释
 *  --------------------------------------
 *  文件名称: CbResourceInstaller.xaml.cs
 *  创建时间: 2023年12月28日 0:14
 *  创建开发:
 *  文件介绍:
 *  --------------------------------------
 */

using CSUL.Models;
using CSUL.Models.Local;
using CSUL.Models.Local.ModPlayer;
using CSUL.Models.Local.ModPlayer.BepInEx;
using CSUL.Models.Network;
using CSUL.Models.Network.CB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CSUL.Windows
{
    /// <summary>
    /// CbResourceInstaller.xaml 的交互逻辑
    /// </summary>
    public partial class CbResourceInstaller : Window
    {
        #region ---模组数据---

        private class ModData : IModData
        {
            public int? Id { get; set; }

            public string? ModVersion { get; set; }

            public string? Description { get; set; }

            public string? ModUrl { get; set; }

            #region ---忽略属性---

            public string Name => throw new NotImplementedException();

            public string ModPath => throw new NotImplementedException();

            public bool IsEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string? LatestVersion => throw new NotImplementedException();

            #endregion ---忽略属性---
        }

        #endregion ---模组数据---

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
                            ModData modData = new()
                            {   //得到模组信息
                                Id = data.Id,
                                ModVersion = data.ResourceVersion,
                                Description = data.Desciption,
                                ModUrl = data.ResourceUrl
                            };
                            CbFileData fileData;
                            if (customInfo.BepInExVersion?.Length > 1)
                            {
                                string pattern = "(b(e?|(ep)?)((in)?(ex)?)?|ex)" + bepVersion.Major;
                                fileData = (data.Files?.FirstOrDefault(x => Regex.IsMatch(x.FileName, pattern, RegexOptions.IgnoreCase)))
                                    ?? throw new Exception("获取模组文件下载地址失败");
                            }
                            else
                            {
                                fileData = data.Files?.FirstOrDefault() ?? throw new Exception("获取模组文件下载地址失败");
                            }
                            using TempDirectory temp = new();
                            string path = Path.Combine(temp.FullName, fileData.FileName);
                            using (Stream stream = File.Create(path)) await NetworkData.DownloadFromUri(fileData.Url, stream, api: true);
                            await player.AddMod(path, modData);
                            MessageBox.Show($"模组{data.Title}自动安装成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToFormative(data.Title), "模组安装失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
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