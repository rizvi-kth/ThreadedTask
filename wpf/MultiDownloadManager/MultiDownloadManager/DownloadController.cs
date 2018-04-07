using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiDownloadManager
{
    public class DownloadController
    {
        InternetClient internetClient;
        Queue<NamedAction> actionList ;

        public DownloadController()
        {
            internetClient = new InternetClient();
            internetClient.SubscribeToReadComplete(InternetClient_ReadComplete);
            //actionList = new List<NamedAction>() {
            //    new NamedAction(){ActionName = "a", CustomAction = async () => await BrowsYahooAsync()},
            //    new NamedAction(){ActionName = "b", CustomAction = async () => await BrowsFacebookAsync()},
            //    new NamedAction(){ActionName = "a", CustomAction = async () => await BrowsYahooAsync()},
            //    new NamedAction(){ActionName = "b", CustomAction = async () => await BrowsFacebookAsync()}
            //};
            actionList = new Queue<NamedAction>();
            actionList.Enqueue(new NamedAction() { ActionName = "a", CustomAction = async () => await BrowsYahooAsync() });
            actionList.Enqueue(new NamedAction() { ActionName = "b", CustomAction = async () => await BrowsFacebookAsync() });
            actionList.Enqueue(new NamedAction() { ActionName = "a", CustomAction = async () => await BrowsYahooAsync() });
            actionList.Enqueue(new NamedAction() { ActionName = "b", CustomAction = async () => await BrowsFacebookAsync() });
            actionList.Enqueue(new NamedAction() { ActionName = "a", CustomAction = async () => await BrowsYahooAsync() });
            actionList.Enqueue(new NamedAction() { ActionName = "b", CustomAction = async () => await BrowsFacebookAsync() });
            actionList.Enqueue(new NamedAction() { ActionName = "a", CustomAction = async () => await BrowsYahooAsync() });
            actionList.Enqueue(new NamedAction() { ActionName = "b", CustomAction = async () => await BrowsFacebookAsync() });




        }

        // Subscription handler
        private async void InternetClient_ReadComplete(object sender, string e)
        {
            Console.Out.WriteLine(e.ToString());
            if (isSequenceMode && (actionList.Count > 0))
                await RunNextInstruction();
        }

        public static bool isSequenceMode = false;
        private static readonly object locked = new object();
        // Shared Execution
        public async Task<HttpResponseMessage> RunNextInstruction()
        {               
                    var p = actionList.Dequeue();
                    var tsk = await p.CustomAction.Invoke();
                    return tsk;
                    //.Result.StatusCode;
                    //Task.Run(()=>p.CustomAction.Invoke()).Wait();
                    //Console.Out.WriteLine("StatusCode" + q.ToString());
        }


        public async Task<HttpResponseMessage> BrowsYahooAsync() => await internetClient.BrowsYahooAsync();
        public async Task<HttpResponseMessage> BrowsFacebookAsync() => await internetClient.BrowsFacebookAsync();

    }

    internal class NamedAction
    {
        public string ActionName { get; set; }

        public Func<Task<HttpResponseMessage>> CustomAction { get; set; }
    }
}
