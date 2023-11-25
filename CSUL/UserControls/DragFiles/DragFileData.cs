using System;
using System.Collections.Generic;

namespace CSUL.UserControls.DragFiles
{
    /// <summary>
    /// 拖动选择文件事件处理委托
    /// </summary>
    public delegate void DragFilesEventHander(object sender, DragFilesEventArgs e);

    /// <summary>
    /// 包含<see cref="DragFilesEventHander"/>的事件数据
    /// </summary>
    public class DragFilesEventArgs : EventArgs
    {
        public DragFilesEventArgs(string[] paths)
        {
            Paths = paths;
        }

        /// <summary>
        /// 选择的文件路径
        /// </summary>
        public string[] Paths { get; } = default!;
    }

    /// <summary>
    /// 拖动文件基本类型
    /// </summary>
    public static class DefaultDragFilesType
    {
        /// <summary>
        /// 压缩文件
        /// </summary>
        public static Dictionary<string, List<string>> ZipFile { get; } = new()
            {
                {"压缩文件",new(){".zip", ".rar", ".7z"} }
            };
    }
}