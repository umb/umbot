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
    //Json Parsing
    using Newtonsoft.Json;
    //REST API
    using System.Net.Http;
    using System.Net.Http.Headers;
    //Array Handling
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #pragma warning disable 1998
    [Serializable]
    public class RootDialog : IDialog<object>
    {

        private string name;

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
                    +"getHealth, deployServer, ping, myip, startJob");
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
                //await this.ping(context);                
                await this.SendPingMessageAsync(context);                
            }
            else if (message.Text.ToLower().Contains("test"))
            {
                await this.restTest(context);
            }
            else if (message.Text.ToLower().Contains("startjob"))
            {
                await this.startJob(context);
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
                await this.ping(context,name); 
                //context.Call(new AgeDialog(this.name), this.AgeDialogResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("I'm sorry, I'm having issues. Let's try again.");

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
        public async Task ping(IDialogContext context, string name)
        {
            //await this.SendPingMessageAsync(context);
            // Ping's the local machine.
            Ping pingSender = new Ping ();
            //string address = "8.8.8.8";
            string address = name;
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
            //string url = "https://reqres.in/api/users?page=3";
            string url = "https://my-json-server.typicode.com/UMB-Linus/test/posts";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                var html = httpClient.GetStringAsync(new Uri(url)).Result;
                //await context.PostAsync(html);
                //dynamic testJ = JsonConvert.DeserializeObject(html);
                //Read JSON
                JsonTextReader reader = new JsonTextReader(new StringReader(html));
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        await context.PostAsync($"Path: {reader.Path},Token: {reader.TokenType}, Value: {reader.Value}");
                        //outList.Add($"Path: {reader.Path},Token: {reader.TokenType}, Value: {reader.Value}");
                    }
                    //else
                    //{
                    //    await context.PostAsync($"Token: {reader.TokenType}");
                    //}
                }
                //await context.PostAsync(outList);
            }
        }
        public async Task startJob(IDialogContext context)
        {
            await context.PostAsync("Starting Job");
            //https://portal.sfcore.ch/engine-rest/engine/process-definition/key/aProcessDefinitionKey/start
            string json = @"{
            'variables': {
                'aProcessVariable' : {
                'value' : 'aStringValue',
                'type': 'String'
                }
            },
            'businessKey' : 'myBusinessKey',
            'skipCustomListeners' : true,
            'startInstructions' :
                [
                {
                    'type': 'startBeforeActivity',
                    'activityId': 'activityId',
                    'variables': {
                    'var': {
                        'value': 'aVariableValue',
                        'local': false,
                        'type': 'String'}
                    }
                },
                {
                    'type': 'startAfterActivity',
                    'activityId': 'anotherActivityId',
                    'variables': {
                    'varLocal': {
                        'value': 'anotherVariableValue',
                        'local': true,
                        'type': 'String'
                    }
                    }
                }
                ]
            }";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    await context.PostAsync($"Path: {reader.Path},Token: {reader.TokenType}, Value: {reader.Value}");
                    //outList.Add($"Path: {reader.Path},Token: {reader.TokenType}, Value: {reader.Value}");
                }
            }
        }
    }
}

