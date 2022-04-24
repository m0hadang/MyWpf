namespace MyNotification.Src
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
    public class GrowlInfo
    {
        public string Message { get; set; }
        public string Description { get; set; }
        public InfoType Type { get; set; }
        public string IconKey { get; set; }
        public string IconBrushKey { get; set; }
        public string IconType { get; set; }
        public int WaitTime { get; set; } = 3;
        public bool ShowDescription { get; set; } = true;
    }
}