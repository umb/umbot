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
                dialogs = new DialogSet();

                // Define our dialog
                dialogs.Add("reserveTable", new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        // Prompt for the guest's name.
                        await dc.Context.SendActivity("Welcome to the reservation service.");

                        await dc.Prompt("dateTimePrompt", "Please provide a reservation date and time.");
                    },
                    async(dc, args, next) =>
                    {
                        var dateTimeResult = ((DateTimeResult)args).Resolution.First();

                        reservationDate = Convert.ToDateTime(dateTimeResult.Value);
                        
                        // Ask for next info
                        await dc.Prompt("partySizePrompt", "How many people are in your party?");

                    },
                    async(dc, args, next) =>
                    {
                        partySize = (int)args["Value"];

                        // Ask for next info
                        await dc.Prompt("textPrompt", "Whose name will this be under?");
                    },
                    async(dc, args, next) =>
                    {
                        reservationName = args["Text"];
                        string msg = "Reservation confirmed. Reservation details - " +
                        $"\nDate/Time: {reservationDate.ToString()} " +
                        $"\nParty size: {partySize.ToString()} " +
                        $"\nReservation name: {reservationName}";
                        await dc.Context.SendActivity(msg);
                        await dc.End();
                    }
                });

                // Add a prompt for the reservation date
                dialogs.Add("dateTimePrompt", new Microsoft.Bot.Builder.Dialogs.DateTimePrompt(Culture.English));
                // Add a prompt for the party size
                dialogs.Add("partySizePrompt", new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(Culture.English));
                // Add a prompt for the user's name
                dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
            }
            else
            {
                await context.PostAsync("I am sorry I don't understand you");
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}