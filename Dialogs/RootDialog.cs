namespace BasicMultiDialogBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Net;
    using System.IO;


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
                    +"getHealth, deployServer");
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
                string html = string.Empty;
                string url = "http://www.myip.ch";

                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }

                //Console.WriteLine(html);
                await context.PostAsync(html);
            }
            else
            {
                await context.PostAsync("I am sorry I don't understand you");
                context.Wait(MessageReceivedAsync);
            }     
        }

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

        private async Task AgeDialogResumeAfter(IDialogContext context, IAwaitable<int> result)
        {
            try
            {
                this.age = await result;

                await context.PostAsync($"Your name is { name } and your age is { age }.");

            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");
            }
            //finally
            //{
            //    await this.SendHealthMessageAsync(context);
            //}
        }
    }
}