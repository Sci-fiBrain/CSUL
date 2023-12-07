using CSUL.UserControls.DragFiles;
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
            DragFile.FileNameWithTypes = DefaultDragFilesType.BepModFile;
        }
    }
}