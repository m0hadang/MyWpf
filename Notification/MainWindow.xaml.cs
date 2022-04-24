using MyNotification.Src;
using System.Windows;
using System.Windows.Input;

namespace Notification
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //리소스 적용한 패널을 전달
            Growl.SetGrowlParent(notifyPanel);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Growl.Info(new GrowlInfo 
            { 
                Message = "Message", 
                Description = "Description" 
            });
            //Growl.Success(new GrowlInfo
            //{
            //    Message = "No Message",
            //    ShowDescription = false,
            //});
        }
    }
}
