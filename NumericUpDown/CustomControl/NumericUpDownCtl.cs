using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NumericUpDown.CustomControl
{
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);

    //ValueChanged Event 인자
    public class ValueChangedEventArgs : RoutedEventArgs
    {
        private int _Value;
        public int Value => _Value;
        public ValueChangedEventArgs(RoutedEvent id, int num)
        {
            _Value = num;
            RoutedEvent = id;//발생되는 Event 정보 설정
        }
    }

    // Control 계약
    // ControlTemplate 작성자가 Template에 무엇을 넣을지 알 수 있도록 Control 계약을 제공
    // dotnet 4.6.1에서 테스트 해본 결과 없어도 동작에는 문제가 없었다.
    //[TemplatePart(Name = "UpButtonElement", Type = typeof(RepeatButton))]
    //[TemplatePart(Name = "DownButtonElement", Type = typeof(RepeatButton))]
    //[TemplateVisualState(Name = "Positive", GroupName = "ValueStates")]
    //[TemplateVisualState(Name = "Negative", GroupName = "ValueStates")]
    //[TemplateVisualState(Name = "Focused", GroupName = "FocusedStates")]
    //[TemplateVisualState(Name = "Unfocused", GroupName = "FocusedStates")]
    public class NumericUpDownCtl : Control
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value), typeof(int), typeof(NumericUpDownCtl),
                new PropertyMetadata(new PropertyChangedCallback(ValueChangedCallback)));
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        private static void ValueChangedCallback(DependencyObject obj,
            DependencyPropertyChangedEventArgs args)
        {
            NumericUpDownCtl ctl = (NumericUpDownCtl)obj;
            int newValue = (int)args.NewValue;

            // Call UpdateStates because the Value might have caused the
            // control to change ValueStates.
            ctl.UpdateStates(true);

            // NumericUpDown의 ValueChanged event 발생
            ctl.OnValueChanged(
                new ValueChangedEventArgs(ValueChangedEvent, newValue));
        }
        protected virtual void OnValueChanged(ValueChangedEventArgs e)
        {
            // ValueChanged event를 구족한 어플리케이션이 알람을 받아서
            // Value가 바뀌었다는 것을 알 수 있도록 이벤트 발생
            RaiseEvent(e);
        }

        // ValueChanged event 선언
        // 어플리케이션에서 ValueChanged event 를 구독할 수 있다.
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ValueChanged),// event 이름
                RoutingStrategy.Direct,// event routing 전략
                typeof(ValueChangedEventHandler),//handler type
                typeof(NumericUpDownCtl));

        public event ValueChangedEventHandler ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        private RepeatButton _DownButtonElement;
        private RepeatButton DownButtonElement
        {
            //private으로 선언해야 외부에서 함부로 접근할 수 없다. 
            //속성은 OnApplyTemplate에서 설정될 것이다.
            get => _DownButtonElement;
            set
            {
                if (_DownButtonElement != null)
                {
                    _DownButtonElement.Click -=
                        new RoutedEventHandler(DownButtonElement_Click);
                }
                _DownButtonElement = value;
                if (_DownButtonElement != null)
                {
                    _DownButtonElement.Click +=
                        new RoutedEventHandler(DownButtonElement_Click);
                }
            }
        }
        void DownButtonElement_Click(object sender, RoutedEventArgs e)
        {
            Value--;
        }


        private RepeatButton _UpButtonElement;
        private RepeatButton UpButtonElement
        {
            get => _UpButtonElement;
            set
            {
                if (_UpButtonElement != null)
                {
                    _UpButtonElement.Click -=
                        new RoutedEventHandler(UpButtonElement_Click);
                }
                _UpButtonElement = value;
                if (_UpButtonElement != null)
                {
                    _UpButtonElement.Click +=
                        new RoutedEventHandler(UpButtonElement_Click);
                }
            }
        }
        void UpButtonElement_Click(object sender, RoutedEventArgs e)
        {
            Value++;
        }
        public NumericUpDownCtl()
        {
            DefaultStyleKey = typeof(NumericUpDownCtl);
            this.IsTabStop = true;
        }
        public override void OnApplyTemplate()
        {
            UpButtonElement = GetTemplateChild("UpButton") as RepeatButton;
            DownButtonElement = GetTemplateChild("DownButton") as RepeatButton;
            //TextElement = GetTemplateChild("TextBlock") as TextBlock;

            UpdateStates(false);
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focus();
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            UpdateStates(true);
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            UpdateStates(true);
        }
        private void UpdateStates(bool useTransitions)
        {
            if (Value >= 0)
            { VisualStateManager.GoToState(this, "Positive", useTransitions); }
            else
            { VisualStateManager.GoToState(this, "Negative", useTransitions); }

            if (IsFocused)
            { VisualStateManager.GoToState(this, "Focused", useTransitions); }
            else
            { VisualStateManager.GoToState(this, "Unfocused", useTransitions); }
        }
    }
}
