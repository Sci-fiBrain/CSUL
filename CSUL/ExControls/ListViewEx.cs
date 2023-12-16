using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CSUL.ExControls
{
    /// <summary>
    /// <see cref="ListView"/>的扩展代码
    /// </summary>
    public static class ListViewEx
    {
        public static readonly DependencyProperty ItemsSourceChangedAnimationProperty =
        DependencyProperty.RegisterAttached(
            "ItemsSourceChangedAnimation", typeof(Storyboard), typeof(ListViewEx),
            new PropertyMetadata(default(Storyboard), ItemsSourceChangedAnimationPropertyChangedCallback));

        public static void SetItemsSourceChangedAnimation(DependencyObject element, Storyboard value)
            => element.SetValue(ItemsSourceChangedAnimationProperty, value);

        public static Storyboard GetItemsSourceChangedAnimation(DependencyObject element)
            => (Storyboard)element.GetValue(ItemsSourceChangedAnimationProperty);

        private static void ItemsSourceChangedAnimationPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is not ListView animatedListView)
                throw new Exception("Can only be applied to an ListView");

            DependencyPropertyDescriptor propertyDescriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListView));
            propertyDescriptor.RemoveValueChanged(animatedListView, ItemsSourceChangedHandler);
            propertyDescriptor.AddValueChanged(animatedListView, ItemsSourceChangedHandler);
        }

        private static void ItemsSourceChangedHandler(object? sender, EventArgs eventArgs)
        {
            if (sender is null) throw new ArgumentNullException(nameof(sender));
            FrameworkElement animateObject = (FrameworkElement)sender;
            Storyboard storyboard = GetItemsSourceChangedAnimation(animateObject);
            storyboard?.Begin(animateObject);
        }
    }
}
