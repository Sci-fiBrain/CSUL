using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSUL.ExControls
{
    /// <summary>
    /// 自定义按钮控件
    /// </summary>
    public class CButton : Button
    {
        static CButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CButton), new FrameworkPropertyMetadata(typeof(CButton)));
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CButton), new PropertyMetadata(null));

        public ImageSource Icon
        {   //图标属性
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty ButtonTypeProperty =
            DependencyProperty.Register("ButtonType", typeof(CButtonType), typeof(CButton), new PropertyMetadata(CButtonType.Icon));

        public CButtonType ButtonType
        {   //按钮类型属性
            get => (CButtonType)GetValue(ButtonTypeProperty);
            set => SetValue(ButtonTypeProperty, value);
        }

        public static readonly DependencyProperty PathDataProperty =
            DependencyProperty.Register("PathData", typeof(Geometry), typeof(CButton), new PropertyMetadata(null));

        public Geometry PathData
        {   //Path类型下的绘制内容
            get => (Geometry)GetValue(PathDataProperty);
            set => SetValue(PathDataProperty, value);
        }
    }
}