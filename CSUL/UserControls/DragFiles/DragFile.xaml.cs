using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CSUL.UserControls.DragFiles
{
    /// <summary>
    /// DragFile.xaml 的交互逻辑
    /// </summary>
    public partial class DragFile : UserControl
    {
        /// <summary>
        /// 文件拖动选择事件
        /// </summary>
        public event DragFilesEventHander? DragEvent;

        public DragFile()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 要筛选的文件类型名称以及对应的文件类型
        /// </summary>
        public Dictionary<string, List<string>> FileNameWithTypes { get; set; } = default!;

        /// <summary>
        /// 要显示的图标路径
        /// </summary>
        public ImageSource Icon
        {
            get => IconImage.Source;
            set => IconImage.Source = value;
        }

        /// <summary>
        /// 要拖动的文件名称
        /// </summary>
        public object FileName
        {
            get => La.Content;
            set => La.Content = $"将{value}拖入此处导入，或点击导入{value}";
        }

        private void MouseLeftButtonUp_Event(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "选择地图文件",
                Multiselect = true,
                Filter = GetFilter(FileNameWithTypes)
            };
            if (dialog.ShowDialog() is true)
            {
                DragEvent?.Invoke(this, new(dialog.FileNames));
            }
        }

        private void Drop_Event(object sender, DragEventArgs e)
        {
            string[] files = DragHandel(e, FileNameWithTypes.SelectMany(x => x.Value).ToArray());
            DragEvent?.Invoke(this, new(files));
        }

        /// <summary>
        /// 拖动处理方法
        /// </summary>
        /// <param name="e">事件数据</param>
        /// <param name="extensions">文件扩展名</param>
        /// <returns>符合条件的文件列表</returns>
        /// <exception cref="ArgumentException">传入的e无效</exception>
        private static string[] DragHandel(DragEventArgs? e, params string[] extensions)
        {
            string[] data = e?.Data?.GetData(DataFormats.FileDrop) as string[]
                ?? throw new ArgumentException("拖动数据无效", nameof(e));
            string[] files = (from file in data where extensions.Any(file.EndsWith) select file).ToArray();
            return files;
        }

        /// <summary>
        /// 获取筛选器字符串
        /// </summary>
        private static string GetFilter(Dictionary<string, List<string>> data)
        {
            IEnumerable<string> types =
                from pair in data
                let ex = from name in pair.Value select '*' + name
                select pair.Key + '|' + string.Join(';', ex);
            return string.Join('|', types);
        }
    }
}