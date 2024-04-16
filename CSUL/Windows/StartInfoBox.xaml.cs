using System.Windows;

namespace CSUL.Windows
{
    /// <summary>
    /// StartInfo.xaml 的交互逻辑
    /// </summary>
    public partial class StartInfoBox : Window
    {
        public StartInfoBox()
        {
            InitializeComponent();
        }

        public string? Text
        {
            get => text.Content.ToString();
            set => text.Content = value;
        }
    }
}