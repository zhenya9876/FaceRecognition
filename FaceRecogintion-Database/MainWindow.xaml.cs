using System.Windows;

namespace FaceRecognition_Database
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
	    public PageAuth pageAuth;
        public MainWindow()
        {
            InitializeComponent();
            pageAuth = new PageAuth();
            frame.NavigationService.Navigate(pageAuth);
        }
    }
}
