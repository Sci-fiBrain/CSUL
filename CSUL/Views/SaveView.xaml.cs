using CSUL.UserControls.DragFiles;
using System.Windows.Controls;

namespace CSUL.Views
{
    /// <summary>
    /// MapView.xaml 的交互逻辑
    /// </summary>
    public partial class SaveView : UserControl
    {
        public SaveView()
        {
            InitializeComponent();
            DragFile.FileNameWithTypes = DefaultDragFilesType.ZipFile;
        }
    }
}