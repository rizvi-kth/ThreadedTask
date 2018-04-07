using MultiDownloadManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiDownloadManagerWpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DownloadController _downloadController;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var cont = await _downloadController.BrowsYahooAsync();
            TextBox1.Text = TextBox1.Text + cont.StatusCode.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _downloadController = new DownloadController();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cont = await _downloadController.BrowsFacebookAsync();
            TextBox2.Text = TextBox2.Text + cont.StatusCode.ToString();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DownloadController.isSequenceMode = true;
            var cont = await _downloadController.RunNextInstruction();
            TextBox3.Text = TextBox3.Text + cont.StatusCode.ToString();

        }
    }
}
