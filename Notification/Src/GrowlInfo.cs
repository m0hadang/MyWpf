using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Notification.Src
{
    public class GrowlInfo
    {
        public string Message { get; set; }

        public string Description { get; set; }

        public bool ShowDescription { get; set; } = true;

        public int WaitTime { get; set; } = 3;

        public string CancelStr { get; set; } = "Cancel String";

        public string ConfirmStr { get; set; } = "Confirm String";

        public Func<bool, bool> ActionBeforeClose { get; set; }

        public bool StaysOpen { get; set; }

        public bool IsCustom { get; set; }

        public InfoType Type { get; set; }

        public string IconKey { get; set; }

        public string IconBrushKey { get; set; }

        public bool ShowCloseButton { get; set; } = true;

        public string Token { get; set; }

        public string IconType { get; set; }
    }

}
