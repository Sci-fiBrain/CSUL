using CSUL.UserControls.DragFiles;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CSUL.Views
{
    /// <summary>
    /// ModView.xaml 的交互逻辑
    /// </summary>
    public partial class ModView : UserControl
    {
        public ModView()
        {
            InitializeComponent();
            Dictionary<string, List<string>> targetType = DefaultDragFilesType.ZipFile;
            targetType.Add("DLL文件", new() { ".dll" });
            DragFile.FileNameWithTypes = DefaultDragFilesType.ZipFile;
        }
    }
}