using CSUL.UserControls.DragFiles;
using System.Windows.Controls;

namespace CSUL.Views
{
    /// <summary>
    /// ModPlayerView.xaml 的交互逻辑
    /// </summary>
    public partial class ModPlayerView : UserControl
    {
        public ModPlayerView()
        {
            InitializeComponent();
            DragFile.FileNameWithTypes = DefaultDragFilesType.BepModFile;
        }
    }
}