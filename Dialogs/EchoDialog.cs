using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using System.Net.Http;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Text == "help")
            {
                await context.PostAsync(
                    "The following commands are supported:\n"
                    +"getHealth\n"
                    +"deployServer");
            }
            else if (message.Text.ToLower().Contains("health"))
            {
                await context.PostAsync(
                    "Blablabla");
            }
            else
            {
                await context.PostAsync("I am sorry I don't understand you");
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}