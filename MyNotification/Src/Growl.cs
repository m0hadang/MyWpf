using MyCustomControl.Command;
using MyCustomControl.Extension;
using MyCustomControl.Helper;
using MyCustomControl.StringGetter;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyNotification.Src
{
    public class Growl : System.Windows.Controls.Control
    {
        #region Element
        private const string ElementPanelMore = "PART_PanelMore";
        private const string ElementGridMain = "PART_GridMain";
        private const string ElementButtonClose = "PART_ButtonClose";
        #endregion

        #region DP Message
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(Growl), new PropertyMetadata(default(string)));
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
        #endregion

        #region DP Description
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(Growl), new PropertyMetadata(default(string)));
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        #endregion

        public Geometry Icon { get; set; }
        public string IconType { get; set; }
        public Brush IconBrush { get; set; }
        public bool ShowDescription { get; set; }
        public InfoType Type { get; set; }

        private Grid _gridMain;
        private Button _buttonClose;
        private DispatcherTimer _timerClose;
        private int _tickCount;
        private bool _showCloseButton;
        private bool _staysOpen;
        private int _waitTime = 6;

        private Func<bool, bool> ActionBeforeClose { get; set; }
        private static Panel GrowlPanel { get; set; }

        //Growl를 StackPanel에 추가하면 UI 다시 그리기 위해 호출
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //Template에서 Element를 검색해서 가져옴
            _gridMain = GetTemplateChild(ElementGridMain) as Grid;
            _buttonClose = GetTemplateChild(ElementButtonClose) as Button;

            if (_gridMain == null || _buttonClose == null)
            {
                throw new Exception();
            }

            Update();
        }

        public Growl()
        {
            void ButtonClose_OnClick(object sender, RoutedEventArgs e) => Close();

            //Binding 설정
            CommandBindings.Add( // 1. binding을 추가하는데
                new CommandBinding(ControlCommands.Close, ButtonClose_OnClick) // 2. binding은 다음과 같다
            ); // 3. 이렇게 CommandBindings 안에 CommandBinding을 추가하면 UIElement의 Command 속성에 ControlCommands.Close을 할당할 수 있다.
            //4. CommandBindings가 이루어 지지 않으면 UI(Close Button)가 비활성화 된다
        }

        public static void SetGrowlParent(Panel growlPanel)
        {
            GrowlPanel = growlPanel;
        }
        public static void Info(GrowlInfo growlInfo)
        {
            InitGrowlInfo(growlInfo, InfoType.Info);
            Show(growlInfo);
        }
        public static void Success(GrowlInfo growlInfo)
        {
            InitGrowlInfo(growlInfo, InfoType.Success);
            Show(growlInfo);
        }
        private static void InitGrowlInfo(GrowlInfo growlInfo, InfoType infoType)
        {
            if (growlInfo == null)
            {
                throw new ArgumentNullException(nameof(growlInfo));
            }
            
            growlInfo.Type = infoType;
            switch (infoType)
            {
                case InfoType.Success:
                    growlInfo.IconKey = ResourceToken.SuccessGeometry;
                    growlInfo.IconBrushKey = ResourceToken.SuccessBrush;
                    growlInfo.IconType = "whitelist";
                    break;
                case InfoType.Info:
                    growlInfo.IconKey = ResourceToken.InfoGeometry;
                    growlInfo.IconBrushKey = ResourceToken.InfoBrush;
                    growlInfo.IconType = "warning";
                    break;
            }
        }
        private static void Show(GrowlInfo growlInfo)
        {
            Application.Current.Dispatcher?.Invoke(
                () =>
                {
                    //생성된 Growl 객체는 GrowlPanel에 추가
                    var ctl = new Growl
                    {
                        Message = growlInfo.Message,
                        Description = growlInfo.Description,
                        Icon = ResourceHelper.GetResource<Geometry>(growlInfo.IconKey),
                        IconType = growlInfo.IconType,
                        IconBrush = ResourceHelper.GetResource<Brush>(growlInfo.IconBrushKey),
                        ShowDescription = growlInfo.ShowDescription,
                        Type = growlInfo.Type,
                        _showCloseButton = true,
                        _staysOpen = false,
                        _waitTime = Math.Max(growlInfo.WaitTime, 2)
                    };
                    GrowlPanel?.Children.Insert(0, ctl);
                }
            );
        }
        //일정 시간 후 Growl가 닫히도록 Timer 설정
        private void StartTimer()
        {
            _timerClose = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timerClose.Tick += delegate
            {
                if (IsMouseOver)
                {
                    _tickCount = 0;
                    return;
                }

                _tickCount++;
                if (_tickCount >= _waitTime) Close(true);
            };
            _timerClose.Start();
        }

        //UI 업데이트
        private void Update()
        {
            //Show 애니메이션 실행
            var transform = new TranslateTransform
            {
                X = MaxWidth
            };
            _gridMain.RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.XProperty, AnimationHelper.CreateAnimation(0));

            if (_staysOpen == false)
            {
                StartTimer();
            }
        }

        //Animation을 이용하여 닫힘 효과 표시
        private void Close(bool invokeActionBeforeClose = false, bool invokeParam = true)
        {
            if (invokeActionBeforeClose)
            {
                if (ActionBeforeClose?.Invoke(invokeParam) == false)// !exp 대신 exp == false를 써야함
                {
                    return;
                }
            }

            _timerClose?.Stop();
            var transform = new TranslateTransform();
            _gridMain.RenderTransform = transform;
            var animation = AnimationHelper.CreateAnimation(ActualWidth);
            animation.Completed += (s, e) =>
            {
                if (Parent is Panel panel)
                {
                    panel.Children.Remove(this);
                }
            };
            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }


        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _buttonClose.Show(_showCloseButton);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _buttonClose.Collapse();
        }

    }
}