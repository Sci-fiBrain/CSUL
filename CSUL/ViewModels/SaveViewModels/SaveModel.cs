using CSUL.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Net.WebRequestMethods;

namespace CSUL.ViewModels.SaveViewModels
{
    /// <summary>
    /// SaveView的ViewModel
    /// </summary>
    public class SaveModel : BaseViewModel
    {
        public class ItemData
        {
            /// <summary>
            /// 存档Id
            /// </summary>
            public string Id { get; set; } = default!;

            /// <summary>
            /// 存档名称
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// 存档路径
            /// </summary>
            public string Path { get; set; } = default!;

            /// <summary>
            /// 最后修改时间
            /// </summary>
            public string LastWriteTime { get; set; } = default!;
        }

        public SaveModel()
        {
            //获取初始数据
            RefreshData();

            //设定命令处理方法
            DropCommand = new RelayCommand((sender) => { 
                string[] files = EventArgsHandels.DragHandel(sender as DragEventArgs, ".zip", ".rar");
                //SaveData = (from file in files select ConvertToItemData(file)).ToList();
            });
            ClickCommand = new RelayCommand((sender) =>
            {
                OpenFileDialog dialog = new()
                {
                    Title = "选择存档文件",
                    Multiselect = true,
                    Filter = "压缩文件|*.zip;*.rar"
                };
                if (dialog.ShowDialog() is true)
                {
                    //SaveData = (from file in dialog.FileNames select ConvertToItemData(file)).ToList();
                }
            });
            DeleteCommand = new RelayCommand((sender) =>
            {
                if (sender is not ItemData data) return;
                StringBuilder sb = new();
                sb.Append("存档名称: ").Append(data.Name).AppendLine();
                sb.Append("存档ID: ").Append(data.Id).AppendLine();
                sb.Append("最后修改时间: ").Append(data.LastWriteTime).AppendLine();
                sb.Append("存档路径: ").AppendLine().Append(data.Path).AppendLine();
                var ret = MessageBox.Show(sb.ToString(), "删除存档", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if(ret == MessageBoxResult.OK)
                {
                    Directory.Delete(data.Path, true);
                    MessageBox.Show("删除成功");
                    RefreshData();
                }
            });
        }

        public ICommand DropCommand { get; }
        public ICommand ClickCommand { get; }
        public ICommand DeleteCommand { get; }

        private List<ItemData> saveData = default!;
        public List<ItemData> SaveData {
            get => saveData;
            set
            {
                if (saveData == value) return;
                saveData = value;
                OnPropertyChanged();
            }
        }

        #region ---私有方法---
        /// <summary>
        /// 将存档文件夹转为条目信息
        /// </summary>
        private static ItemData ConvertToItemData(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException();
            DirectoryInfo info = new(path);
            ItemData data = new()
            {
                Id = info.Name,
                Name = info.GetFiles().FirstOrDefault(x => x.Name.EndsWith(".cok"))?.Name.Split('.')[0],
                Path = path,
                LastWriteTime = info.LastWriteTime.ToString("yyyy-MM-dd-HH:mm:ss"),
            };
            return data;
        }

        /// <summary>
        /// 刷新存档数据
        /// </summary>
        private void RefreshData() => SaveData = (from dir in FileManager.Instance.SaveDir.GetDirectories() select ConvertToItemData(dir.FullName)).ToList();
        #endregion
    }
}
