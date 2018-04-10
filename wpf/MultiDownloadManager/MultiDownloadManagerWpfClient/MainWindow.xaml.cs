using MultiDownloadManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));
        DownloadController _downloadController;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var cont = await _downloadController.BrowsYahooAsync();
            TextBox1.Text = TextBox1.Text + cont.StatusCode.ToString();
            log.Info("Browsing Yahoo Successful.");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _downloadController = new DownloadController();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cont = await _downloadController.BrowsFacebookAsync();
            TextBox2.Text = TextBox2.Text + cont.StatusCode.ToString();
            log.Info("Browsing Facebook Successful.");
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DownloadController.isSequenceMode = true;
            var cont = await _downloadController.RunNextInstruction();
            TextBox3.Text = TextBox3.Text + cont.StatusCode.ToString();

        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpResponseMessage cont = await _downloadController.BrowsEmptyAsync();
                TextBox2.Text = TextBox2.Text + cont.StatusCode.ToString();
            }
            catch (Exception e1)
            {
                // Here it will be a HttpRequestException which is the last exception thrown.
                log.Info(e1);
                //throw;
            }
        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                Task<HttpResponseMessage> _failTask = _downloadController.BrowsEmptyAsync();
                await _failTask.ContinueWith((p)=> {  }, TaskContinuationOptions.ExecuteSynchronously);
                _failTask.Wait();
                //HttpResponseMessage cont = t.Result;
            }
            catch (AggregateException e1)
            {
                // Prefered way of exception handling.
                // Here it will be a AggregateException which is a collection of all the Tasks failed in the Tack-Collection.
                log.Info(e1);
                //throw;
            }
        }
    }
}
