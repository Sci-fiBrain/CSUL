using System;
using System.Collections.Generic;
using System.Linq;

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
    /// 拖动文件类型
    /// </summary>
    public class DragFilesType : Dictionary<string, List<string>>
    {
        public DragFilesType()
        { }

        public DragFilesType(IEnumerable<KeyValuePair<string, List<string>>> collection, IEqualityComparer<string>? comparer = null) : base(collection, comparer)
        {
        }

        /// <summary>
        /// 创建“所有合法文件”条目
        /// </summary>
        public DragFilesType AddAllFileItem()
        {
            DragFilesType ret = new()
            {
                { "合法文件", this.SelectMany(x => x.Value).ToList() }
            };
            return new(ret.Concat(this));
        }
    }

    /// <summary>
    /// 拖动文件基本类型
    /// </summary>
    public static class DefaultDragFilesType
    {
        #region ---私有字段---

        private static readonly DragFilesType zipFile = new()
            {
                {"压缩文件", new (){".zip", ".rar", ".7z", ".tar"}
}
            };

        private static readonly DragFilesType bepModFile = new()
        {
            {"压缩文件", new(){".zip", ".rar", ".7z", ".tar"} },
            {"DLL文件", new() {".dll" } }
        };

        #endregion ---私有字段---

        #region ---公共属性---

        /// <summary>
        /// 压缩文件
        /// </summary>
        public static DragFilesType ZipFile { get => zipFile.AddAllFileItem(); }

        /// <summary>
        /// BepMod文件
        /// </summary>
        public static DragFilesType BepModFile { get => bepModFile.AddAllFileItem(); }

        #endregion ---公共属性---
    }
}