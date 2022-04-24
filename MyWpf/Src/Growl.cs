using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Notification.BehaviorSrc;
using Notification.UIControl;

namespace Notification.Src
{
    public enum InfoType
    {
        Success = 0,
        Info,
        Warning,
        Error,
        Fatal,
        Ask
    }

    [TemplatePart(Name = ElementPanelMore, Type = typeof(Panel))]
    [TemplatePart(Name = ElementGridMain, Type = typeof(Grid))]
    [TemplatePart(Name = ElementButtonClose, Type = typeof(Button))]
    public class Growl : System.Windows.Controls.Control
    {
        #region Constants

        private const string ElementPanelMore = "PART_PanelMore";
        private const string ElementGridMain = "PART_GridMain";
        private const string ElementButtonClose = "PART_ButtonClose";

        #endregion Constants

        #region Data

        private Panel _panelMore;

        private Grid _gridMain;

        private Button _buttonClose;

        private bool _showCloseButton;

        private bool _staysOpen;

        private int _waitTime = 6;

        private int _tickCount;

        private DispatcherTimer _timerClose;

        private static readonly Dictionary<string, Panel> PanelDic = new Dictionary<string, Panel>();

        #endregion Data

        public Growl()
        {
            //Binding 설정
            CommandBindings.Add( // 1. binding을 추가하는데
                new CommandBinding(ControlCommands.Close, ButtonClose_OnClick) // 2. binding은 다음과 같다
            ); // 3. 이렇게 CommandBindings 안에 CommandBinding을 추가하면 UIElement의 Command 속성에 ControlCommands.Close을 할당할 수 있다.
            CommandBindings.Add(new CommandBinding(ControlCommands.Cancel, ButtonCancel_OnClick));
            CommandBindings.Add(new CommandBinding(ControlCommands.Confirm, ButtonOk_OnClick));
        }

        public static void Register(string token, Panel panel)
        {
            if (string.IsNullOrEmpty(token) || panel == null) return;
            PanelDic[token] = panel;
            InitGrowlPanel(panel);
        }

        public static void Unregister(string token, Panel panel)
        {
            if (string.IsNullOrEmpty(token) || panel == null) return;

            if (PanelDic.ContainsKey(token))
            {
                if (ReferenceEquals(PanelDic[token], panel))
                {
                    PanelDic.Remove(token);
                    panel.ContextMenu = null;
                    panel.SetCurrentValue(PanelElement.FluidMoveBehaviorProperty, DependencyProperty.UnsetValue);
                }
            }
        }

        public static void Unregister(Panel panel)
        {
            if (panel == null) return;
            var first = PanelDic.FirstOrDefault(item => ReferenceEquals(panel, item.Value));
            if (!string.IsNullOrEmpty(first.Key))
            {
                PanelDic.Remove(first.Key);
                panel.ContextMenu = null;
                panel.SetCurrentValue(PanelElement.FluidMoveBehaviorProperty, DependencyProperty.UnsetValue);
            }
        }

        public static void Unregister(string token)
        {
            if (string.IsNullOrEmpty(token)) return;

            if (PanelDic.ContainsKey(token))
            {
                var panel = PanelDic[token];
                PanelDic.Remove(token);
                panel.ContextMenu = null;
                panel.SetCurrentValue(PanelElement.FluidMoveBehaviorProperty, DependencyProperty.UnsetValue);
            }
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
        //Growl를 StackPanel에 추가하면 UI 다시 그리기 위해 호출
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //Template에서 Element를 검색해서 가져옴
            _panelMore = GetTemplateChild(ElementPanelMore) as Panel;
            _gridMain = GetTemplateChild(ElementGridMain) as Grid;
            _buttonClose = GetTemplateChild(ElementButtonClose) as Button;

            CheckNull();
            Update();
        }

        private void CheckNull()
        {
            if (_panelMore == null || _gridMain == null || _buttonClose == null) throw new Exception();
        }

        private Func<bool, bool> ActionBeforeClose { get; set; }

        public static Panel GrowlPanel { get; set; }

        internal static readonly DependencyProperty CancelStrProperty = DependencyProperty.Register(
            "CancelStr", typeof(string), typeof(Growl), new PropertyMetadata(default(string)));

        internal static readonly DependencyProperty ConfirmStrProperty = DependencyProperty.Register(
            "ConfirmStr", typeof(string), typeof(Growl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ShowDateTimeProperty = DependencyProperty.Register(
            "ShowDescription", typeof(bool), typeof(Growl), new PropertyMetadata(ValueBoxes.TrueBox));

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(Growl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(Growl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time", typeof(DateTime), typeof(Growl), new PropertyMetadata(default(DateTime)));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(Geometry), typeof(Growl), new PropertyMetadata(default(Geometry)));

        public static readonly DependencyProperty IconTypeProperty = DependencyProperty.Register(
            "IconType", typeof(string), typeof(Growl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            "IconBrush", typeof(Brush), typeof(Growl), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(InfoType), typeof(Growl), new PropertyMetadata(default(InfoType)));

        //Attach Property
        public static readonly DependencyProperty GrowlParentProperty = DependencyProperty.RegisterAttached(
            "GrowlParent", typeof(bool), typeof(Growl), new PropertyMetadata(ValueBoxes.FalseBox, (o, args) =>
            {
                //true로 설정되면 true로 설정한 자식 element가 o 로 전달됨
                //o 로 전달되는 property는 SetGrowlParent를 통해 전달한 StackPanel
                if ((bool)args.NewValue && o is Panel panel)
                {
                    SetGrowlPanel(panel);
                }
            }));

        public static readonly DependencyProperty TokenProperty = DependencyProperty.RegisterAttached(
            "Token", typeof(string), typeof(Growl), new PropertyMetadata(default(string), OnTokenChanged));

        private static void OnTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Panel panel)
            {
                if (e.NewValue == null)
                {
                    Unregister(panel);
                }
                else
                {
                    Register(e.NewValue.ToString(), panel);
                }
            }
        }

        public static void SetToken(DependencyObject element, string value)
            => element.SetValue(TokenProperty, value);

        public static string GetToken(DependencyObject element)
            => (string)element.GetValue(TokenProperty);

        /* 
         * 전달된 element의 DependencyProperty 속성을 설정
         * 
         * Button button = new Button();
         * button.Content = "button";
         * DockPanel dockPanel = new DockPanel(); 
         * dockPanel.Children.Add(button); 
         * button.SetValue(DockPanel.DockProperty, Dock.Top);
         * 
         * GrowlParentProperty는 Attach Property라서 element에 Attach됨
         * element의 GrowlParent Property 값을 value로 설정한 것과 같음
         * 
         * ?? 왜 static으로 StackPanel을 설정하지 않는 걸까??
         */
        public static void SetGrowlParent(DependencyObject element, bool value)
            => element.SetValue(GrowlParentProperty, ValueBoxes.BooleanBox(value));

        public static bool GetGrowlParent(DependencyObject element)
            => (bool)element.GetValue(GrowlParentProperty);

        public InfoType Type
        {
            get => (InfoType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        internal string CancelStr
        {
            get => (string)GetValue(CancelStrProperty);
            set => SetValue(CancelStrProperty, value);
        }

        internal string ConfirmStr
        {
            get => (string)GetValue(ConfirmStrProperty);
            set => SetValue(ConfirmStrProperty, value);
        }

        public bool ShowDescription
        {
            get => (bool)GetValue(ShowDateTimeProperty);
            set => SetValue(ShowDateTimeProperty, ValueBoxes.BooleanBox(value));
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public DateTime Time
        {
            get => (DateTime)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public Geometry Icon
        {
            get => (Geometry)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string IconType
        {
            get => (string)GetValue(IconTypeProperty);
            set => SetValue(IconTypeProperty, value);
        }

        public Brush IconBrush
        {
            get => (Brush)GetValue(IconBrushProperty);
            set => SetValue(IconBrushProperty, value);
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

        //static 메서드, 한번만 호출
        private static void SetGrowlPanel(Panel panel)
        {
            GrowlPanel = panel;//GrowlPanel는 static 변수, 한번만 설정됨, 모든 Growl이 공유
            InitGrowlPanel(panel);
        }

        //static 메서드, 한번만 호출, 패널을 초기화
        private static void InitGrowlPanel(Panel panel)
        {
            if (panel == null) return;

            //Clear라는 MenuItem 추가
            var menuItem = new MenuItem();
            menuItem.Header = "Clear";

            //MenuItem 클릭하였을때 자식 Element에서 Growl라는 Element들을 모두 찾아서 Close 호출
            //이 코드로 보아 StackPanel에 Growl가 자식으로서 추가된다.
            //Growl가 보이는 것은 이 Panel에 Growl가 추가됨으로서 보이는 것이다.
            menuItem.Click += (s, e) =>
            {
                foreach (var item in panel.Children.OfType<Growl>())
                {
                    item.Close();
                }
            };
            //Panel의 ContextMenu
            panel.ContextMenu = new ContextMenu
            {
                Items =
                {
                    menuItem
                }
            };
            //리소스를 가져옴
            var res = ResourceHelper.GetResourceDic(
                "/BehaviorXaml/Behaviors.xaml");
            //리소스 안에 BehaviorXY400 라는 Behavior가 있는지 검색
            if (res.Contains(ResourceToken.BehaviorXY400))
            {
                //있으면 BehaviorXY400 Behavior를 panel에 설정
                //ShadowBehaviors, FluidMoveBehavior 를 StackPanel에 설정
                PanelElement.SetFluidMoveBehavior(panel, res[ResourceToken.BehaviorXY400] as FluidMoveBehavior);
            }
        }

        //UI 업데이트
        private void Update()
        {
            if (Type == InfoType.Ask)
            {
                _panelMore.IsEnabled = true;
                _panelMore.Show();
            }

            //애니메이션 실행
            var transform = new TranslateTransform
            {
                X = MaxWidth
            };
            _gridMain.RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.XProperty, AnimationHelper.CreateAnimation(0));
            if (!_staysOpen) StartTimer();
        }

        //growlInfo 객체로 Growl 객체 초기화
        //생성된 Growl을 GrowlPanel(StackPanel)에 추가
        private static void Show(GrowlInfo growlInfo)
        {
            Application.Current.Dispatcher?.Invoke(
#if NET40
                new Action(
#endif
                    () =>
                    {
                        //생성된 Growl 객체는 GrowlPanel에 추가
                        var ctl = new Growl
                        {
                            Message = growlInfo.Message,
                            Description = growlInfo.Description,
                            Time = DateTime.Now,
                            Icon = ResourceHelper.GetResource<Geometry>(growlInfo.IconKey),
                            IconType = growlInfo.IconType,
                            IconBrush = ResourceHelper.GetResource<Brush>(growlInfo.IconBrushKey),
                            _showCloseButton = growlInfo.ShowCloseButton,
                            ActionBeforeClose = growlInfo.ActionBeforeClose,
                            _staysOpen = growlInfo.StaysOpen,
                            ShowDescription = growlInfo.ShowDescription,
                            ConfirmStr = growlInfo.ConfirmStr,
                            CancelStr = growlInfo.CancelStr,
                            Type = growlInfo.Type,
                            _waitTime = Math.Max(growlInfo.WaitTime, 2)
                        };
                        if (!string.IsNullOrEmpty(growlInfo.Token))
                        {
                            if (PanelDic.TryGetValue(growlInfo.Token, out var panel))
                            {
                                panel?.Children.Insert(0, ctl);
                            }
                        }
                        else
                        {
                            GrowlPanel?.Children.Insert(0, ctl);
                        }
                    }
#if NET40
                )
#endif
            );
        }

        //Growl를 화면에 출력하기 전에 호출, 사전 작업
        //growlInfo는 클라이언트 코드에서 호출할때 생성해서 전달
        //growlInfo에 추가적인 정보 설정
        private static void InitGrowlInfo(ref GrowlInfo growlInfo, InfoType infoType)
        {
            if (growlInfo == null) throw new ArgumentNullException(nameof(growlInfo));
            growlInfo.Type = infoType;

            switch (infoType)
            {
                case InfoType.Success:
                    if (!growlInfo.IsCustom)
                    {
                        growlInfo.IconKey = ResourceToken.SuccessGeometry;
                        growlInfo.IconBrushKey = ResourceToken.SuccessBrush;
                        growlInfo.IconType = "whitelist";
                    }
                    else
                    {
                        if (growlInfo.IconKey == null)
                        {
                            growlInfo.IconKey = ResourceToken.SuccessGeometry;
                        }
                        if (growlInfo.IconBrushKey == null)
                        {
                            growlInfo.IconBrushKey = ResourceToken.SuccessBrush;
                        }
                    }
                    break;
                case InfoType.Info:
                    if (!growlInfo.IsCustom)
                    {
                        growlInfo.IconKey = ResourceToken.InfoGeometry;
                        growlInfo.IconBrushKey = ResourceToken.InfoBrush;
                        growlInfo.IconType = "warning";
                    }
                    else
                    {
                        if (growlInfo.IconKey == null)
                        {
                            growlInfo.IconKey = ResourceToken.InfoGeometry;
                        }
                        if (growlInfo.IconBrushKey == null)
                        {
                            growlInfo.IconBrushKey = ResourceToken.InfoBrush;
                        }
                    }
                    break;
                case InfoType.Warning:
                    if (!growlInfo.IsCustom)
                    {
                        growlInfo.IconKey = ResourceToken.WarningGeometry;
                        growlInfo.IconBrushKey = ResourceToken.WarningBrush;
                    }
                    else
                    {
                        if (growlInfo.IconKey == null)
                        {
                            growlInfo.IconKey = ResourceToken.WarningGeometry;
                        }
                        if (growlInfo.IconBrushKey == null)
                        {
                            growlInfo.IconBrushKey = ResourceToken.WarningBrush;
                        }

                    }
                    break;
                case InfoType.Error:
                    if (!growlInfo.IsCustom)
                    {
                        growlInfo.IconKey = ResourceToken.ErrorGeometry;
                        growlInfo.IconBrushKey = ResourceToken.DangerBrush;
                        growlInfo.StaysOpen = true;
                    }
                    else
                    {
                        if (growlInfo.IconKey == null)
                        {
                            growlInfo.IconKey = ResourceToken.ErrorGeometry;
                        }
                        if (growlInfo.IconBrushKey == null)
                        {
                            growlInfo.IconBrushKey = ResourceToken.DangerBrush;
                        }

                    }
                    break;
                case InfoType.Fatal:
                    if (!growlInfo.IsCustom)
                    {
                        growlInfo.IconKey = ResourceToken.FatalGeometry;
                        growlInfo.IconBrushKey = ResourceToken.PrimaryTextBrush;
                        growlInfo.StaysOpen = true;
                        growlInfo.ShowCloseButton = false;
                        Application.Current.Dispatcher?.Invoke(
#if NET40
                            new Action(
#endif
                                () =>
                                {
                                    if (GrowlPanel.ContextMenu != null) GrowlPanel.ContextMenu.Opacity = 0;
                                }
#if NET40
                            )
#endif
                        );
                    }
                    else
                    {
                        if (growlInfo.IconKey == null)
                        {
                            growlInfo.IconKey = ResourceToken.FatalGeometry;
                        }
                        if (growlInfo.IconBrushKey == null)
                        {
                            growlInfo.IconBrushKey = ResourceToken.PrimaryTextBrush;
                        }

                    }
                    break;
                case InfoType.Ask:
                    growlInfo.StaysOpen = true;
                    growlInfo.ShowCloseButton = false;
                    if (!growlInfo.IsCustom)
                    {
                        growlInfo.IconKey = ResourceToken.AskGeometry;
                        growlInfo.IconBrushKey = ResourceToken.AccentBrush;
                    }
                    else
                    {
                        if (growlInfo.IconKey == null)
                        {
                            growlInfo.IconKey = ResourceToken.AskGeometry;
                        }
                        if (growlInfo.IconBrushKey == null)
                        {
                            growlInfo.IconBrushKey = ResourceToken.AccentBrush;
                        }

                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(infoType), infoType, null);
            }
        }

        public static void Success(string message, string token = "") => Success(new GrowlInfo
        {
            Message = message,
            Token = token
        });

        public static void Success(GrowlInfo growlInfo)
        {
            InitGrowlInfo(ref growlInfo, InfoType.Success);
            Show(growlInfo);
        }

        public static void Info(string message, string token = "") => Info(new GrowlInfo
        {
            Message = message,
            Token = token
        });

        public static void Info(GrowlInfo growlInfo)
        {
            InitGrowlInfo(ref growlInfo, InfoType.Info);
            Show(growlInfo);
        }

        public static void Warning(string message, string token = "") => Warning(new GrowlInfo
        {
            Message = message,
            Token = token
        });

        public static void Warning(GrowlInfo growlInfo)
        {
            InitGrowlInfo(ref growlInfo, InfoType.Warning);
            Show(growlInfo);
        }

        public static void Error(string message, string token = "") => Error(new GrowlInfo
        {
            Message = message,
            Token = token
        });

        public static void Error(GrowlInfo growlInfo)
        {
            InitGrowlInfo(ref growlInfo, InfoType.Error);
            Show(growlInfo);
        }

        public static void Fatal(string message, string token = "") => Fatal(new GrowlInfo
        {
            Message = message,
            Token = token
        });

        public static void Fatal(GrowlInfo growlInfo)
        {
            InitGrowlInfo(ref growlInfo, InfoType.Fatal);
            Show(growlInfo);
        }

        public static void Ask(string message, Func<bool, bool> actionBeforeClose, string token = "") => Ask(new GrowlInfo
        {
            Message = message,
            ActionBeforeClose = actionBeforeClose,
            Token = token
        });

        public static void Ask(GrowlInfo growlInfo)
        {
            InitGrowlInfo(ref growlInfo, InfoType.Ask);
            Show(growlInfo);
        }


        //Animation을 이용하여 닫힘 효과 표시
        private void Close(bool invokeActionBeforeClose = false, bool invokeParam = true)
        {
            if (invokeActionBeforeClose)
            {
                if (ActionBeforeClose?.Invoke(invokeParam) == false) return;
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

        public static void Clear(string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                if (PanelDic.TryGetValue(token, out var panel))
                {
                    Clear(panel);
                }
            }
            else
            {
                Clear(GrowlPanel);
            }
        }

        private static void Clear(Panel panel)
        {
            if (panel == null) return;
            panel.Children.Clear();
            if (panel.ContextMenu != null)
            {
                panel.ContextMenu.IsOpen = false;
                panel.ContextMenu.Opacity = 1;
            }
        }
        private void ButtonClose_OnClick(object sender, RoutedEventArgs e) => Close();
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e) => Close(true, false);
        private void ButtonOk_OnClick(object sender, RoutedEventArgs e) => Close(true);
    }
}
