using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyCustomControlLib.Helper;
using MyCustomControlLib.StringGetter;
namespace MyNotification.Src
{
    public class Growl : System.Windows.Controls.Control
    {
        private bool _showCloseButton;
        private bool _staysOpen;
        private int _waitTime = 6;

        public string Message { get; set; }
        public string Description { get; set; }
        public Geometry Icon { get; set; }
        public string IconType { get; set; }
        public Brush IconBrush { get; set; }
        public bool ShowDescription { get; set; }
        public InfoType Type { get; set; }

        private static Panel GrowlPanel { get; set; }
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
    }
}