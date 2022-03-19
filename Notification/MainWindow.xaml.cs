using Notification.Src;
using System.Windows;

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
            Growl.SetGrowlParent(notifyPanel, true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Growl.Info(new GrowlInfo 
            { 
                Message = "Message", 
                Description = "Description" 
            });
        }
    }
}
