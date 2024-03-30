using CSUL.UserControls.DragFiles;
using System.Windows.Controls;

namespace CSUL.Views
{
    /// <summary>
    /// AssetView.xaml 的交互逻辑
    /// </summary>
    public partial class AssetView : UserControl
    {
        public AssetView()
        {
            InitializeComponent();
            DragFile.FileNameWithTypes = DefaultDragFilesType.GameFile;
        }
    }
}