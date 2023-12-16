using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CSUL.ExControls
{
    /// <summary>
    /// <see cref="ContentControl"/>的附加属性
    /// </summary>
    public static class ContentControlEx
    {
        public static readonly DependencyProperty ContentChangedAnimationProperty = DependencyProperty.RegisterAttached(
    "ContentChangedAnimation", typeof(Storyboard), typeof(ContentControlEx),
    new PropertyMetadata(default(Storyboard), ContentChangedAnimationPropertyChangedCallback));

        public static void SetContentChangedAnimation(DependencyObject element, Storyboard value)
            => element.SetValue(ContentChangedAnimationProperty, value);

        public static Storyboard GetContentChangedAnimation(DependencyObject element)
            => (Storyboard)element.GetValue(ContentChangedAnimationProperty);

        private static void ContentChangedAnimationPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is not ContentControl contentControl)
                throw new Exception("Can only be applied to a ContentControl");
            DependencyPropertyDescriptor propertyDescriptor = DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty,
                typeof(ContentControl));
            propertyDescriptor.RemoveValueChanged(contentControl, ContentChangedHandler);
            propertyDescriptor.AddValueChanged(contentControl, ContentChangedHandler);
        }

        private static void ContentChangedHandler(object? sender, EventArgs eventArgs)
        {
            if (sender is null) throw new ArgumentNullException(nameof(sender));
            FrameworkElement animateObject = (FrameworkElement)sender;
            Storyboard storyboard = GetContentChangedAnimation(animateObject);
            storyboard.Begin(animateObject);
        }
    }
}