using MyCustomControl.Helper;
using MyCustomControl.ValueConverter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyCustomControl.Attache
{
    public class BorderElement
    {
        //외부에 CornerRadius 속성을 붙여서 사용할 수 있도록 한다.
        #region CornerRadius
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(CornerRadius), typeof(BorderElement), 
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.Inherits));
        public static void SetCornerRadius(DependencyObject element, CornerRadius value) 
            => element.SetValue(CornerRadiusProperty, value);
        public static CornerRadius GetCornerRadius(DependencyObject element) 
            => (CornerRadius)element.GetValue(CornerRadiusProperty);
        #endregion

        #region Circular
        public static readonly DependencyProperty CircularProperty = DependencyProperty.RegisterAttached(
            "Circular", typeof(bool), typeof(BorderElement), 
            new PropertyMetadata(ValueBoxes.FalseBox, OnCircularChanged));
        public static void SetCircular(DependencyObject element, bool value) 
            => element.SetValue(CircularProperty, ValueBoxes.BooleanBox(value));
        public static bool GetCircular(DependencyObject element)
            => (bool)element.GetValue(CircularProperty);
        private static void OnCircularChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                if ((bool)e.NewValue)
                {
                    var binding = new MultiBinding
                    {
                        Converter = new BorderCircularConverter()
                    };
                    binding.Bindings.Add(new Binding(FrameworkElement.ActualWidthProperty.Name) { Source = border });
                    binding.Bindings.Add(new Binding(FrameworkElement.ActualHeightProperty.Name) { Source = border });
                    border.SetBinding(Border.CornerRadiusProperty, binding);
                }
                else
                {
                    BindingOperations.ClearBinding(border, FrameworkElement.ActualWidthProperty);
                    BindingOperations.ClearBinding(border, FrameworkElement.ActualHeightProperty);
                    BindingOperations.ClearBinding(border, Border.CornerRadiusProperty);
                }
            }
        }
        #endregion
    }
}
