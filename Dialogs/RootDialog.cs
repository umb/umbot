namespace BasicMultiDialogBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    //Used for WebCalls
    using System.Net;
    using System.IO;
    //Used for ICMP
    using System.Net.NetworkInformation;


    #pragma warning disable 1998

    [Serializable]
    public class RootDialog : IDialog<object>
    {

        private string name;
        private int age;

        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
             *  to process that message. */
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */
            var message = await argument;
            if (message.Text == "help")
            {
                await context.PostAsync(
                    "The following commands are supported:\n\n"
                    +"getHealth, deployServer, ping, myip");
            }
            else if (message.Text.ToLower().Contains("health"))
            {
                await this.SendHealthMessageAsync(context);
            }
            else if(message.Text.ToLower().Contains("client") || message.Text.ToLower().Contains("customer"))
            {
                await context.PostAsync(
                    "The following customers are available:\n\n"
                    +"AMWA, PSS, CIQ");
            }
            else if (message.Text.ToLower().Contains("myip"))
            {
                await this.myip(context);
            }
            else if (message.Text.ToLower().Contains("ping"))
            {
                await this.ping(context);                
            }
            else if (message.Text.ToLower().Contains("test"))
            {
                await this.restTest(context);
            }
            else
            {
                await context.PostAsync("I am sorry I don't understand you");
                context.Wait(MessageReceivedAsync);
            }     
        }

        //Let's Ping Something
        private async Task SendPingMessageAsync(IDialogContext context)
        {
            context.Call(new PingDialog(), this.PingDialogResumeAfter);
        }
        private async Task PingDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.name = await result;

                await context.PostAsync($"Loading Ping metrics for: { name }.");

                //context.Call(new AgeDialog(this.name), this.AgeDialogResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.SendPingMessageAsync(context);
            }
        }


        //Waterfall Dialog for Health Check
        private async Task SendHealthMessageAsync(IDialogContext context)
        {
            //await context.PostAsync("Which Service do you want to inspect?");

            context.Call(new ServiceDialog(), this.HealthDialogResumeAfter);
        }

        private async Task HealthDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.name = await result;

                await context.PostAsync($"Loading Health metrics for: { name }.");

                //context.Call(new AgeDialog(this.name), this.AgeDialogResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await this.SendHealthMessageAsync(context);
            }
        }


        //Ping IP (Debugging Tool)
        public async Task ping(IDialogContext context)
        {
            //await this.SendPingMessageAsync(context);
            // Ping's the local machine.
            Ping pingSender = new Ping ();
            string address = "8.8.8.8";
            PingReply reply = pingSender.Send (address);

            if (reply.Status == IPStatus.Success)
            {
                await context.PostAsync("Success");
            }
            else
            {
                //Console.WriteLine (reply.Status);
                await context.PostAsync("Failure");
            }
        }

        //Lookup my IP (Debugging Tool)
        public async Task myip(IDialogContext context)
        {
            string url = "http://www.myip.ch";
            using (WebClient wc = new WebClient())
            {
            var html = wc.DownloadString(url);
            await context.PostAsync(html);
            }
        }

        //Rest Test (Debugging Tool)
        public async Task restTest(IDialogContext context)
        {
            string url = "https://reqres.in/api/users?page=3";
            using (WebClient wc = new WebClient())
            {
            var html = wc.DownloadString(url);
            await context.PostAsync(html);
            }
        }
    }
}

