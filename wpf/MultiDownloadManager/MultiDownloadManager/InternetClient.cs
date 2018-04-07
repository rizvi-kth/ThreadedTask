using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;


namespace MultiDownloadManager
{

    public class InternetClient
    {
        
        public event EventHandler<string> ReadComplete;

        public void RaiseReadComplete(string content)
        {
            ReadComplete?.Invoke(this, content);
        }

        public void SubscribeToReadComplete(Action<object, string> func)
        {
            this.ReadComplete += func.Invoke;
        }

        

        public InternetClient()
        {
            
        }

        public async Task<HttpResponseMessage> BrowsYahooAsync()
        {
            HttpClient httpClient;
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(3000);

            httpClient.BaseAddress = new Uri("http://www.yahoo.com");
            //RaiseReadComplete("<Just Called Yahoo done>");
            var p = await httpClient.GetAsync("");
            RaiseReadComplete("<Yahoo response > " + p.StatusCode.ToString());
            return p;

        }
        
        public async Task<HttpResponseMessage> BrowsFacebookAsync()
        {
            HttpClient httpClient;
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(3000);

            httpClient.BaseAddress = new Uri("http://www.facebook.com/");
            //RaiseReadComplete("<Just Called Facebook done>"); 
            var p = await httpClient.GetAsync("");
            // When this event is raised is Inportant (???)
            // Should be after await (???)
            RaiseReadComplete("<Facebook response > " + p.StatusCode.ToString()); 
            return p;

        }

    }


}
