using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSUL.Models
{
    /// <summary>
    /// 事件数据处理方法类
    /// </summary>
    public static class EventArgsHandels
    {
        /// <summary>
        /// 拖动处理方法
        /// </summary>
        /// <param name="e">事件数据</param>
        /// <param name="extensions">文件扩展名</param>
        /// <returns>符合条件的文件列表</returns>
        /// <exception cref="ArgumentException">传入的e无效</exception>
        public static string[] DragHandel(DragEventArgs? e, params string[] extensions)
        {
            string[] data = e?.Data?.GetData(DataFormats.FileDrop) as string[]
                ?? throw new ArgumentException("拖动数据无效", nameof(e));
            string[] files = (from file in data where extensions.Any(file.EndsWith) select file).ToArray();
            return files;
        }
    }
}
