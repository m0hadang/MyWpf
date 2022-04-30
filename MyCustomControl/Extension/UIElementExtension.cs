using System.Windows;

namespace MyCustomControl.Extension
{
    public static class UIElementExtension
    {
        public static void Show(this UIElement element) => element.Visibility = Visibility.Visible;
        public static void Show(this UIElement element, bool show) => element.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        public static void Hide(this UIElement element) => element.Visibility = Visibility.Hidden;
        public static void Collapse(this UIElement element) => element.Visibility = Visibility.Collapsed;
    }
}
