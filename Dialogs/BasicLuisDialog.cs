using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Gretting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello there, nice to meet you");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Entertainment.Search")]
        public async Task SearchIntent(IDialogContext context, LuisResult result)
        {
            string query = "";
            string imageURL = "http://www.theoldrobots.com/images62/Bender-18.JPG";

            if (result.Entities != null && result.Entities.Count > 0)
            {
                query = result.Entities[0].Entity;

                //call API to get robohash image
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("X-Mashape-Key", "E2FDfvmYYNmshbiepUhzSpBNkoW5p1Eif8Tjsnf84Ux8VnEGoR");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("https://robohash.p.mashape.com/index.php?text=" + query);
                if (response.IsSuccessStatusCode)
                {
                    dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    imageURL = obj["imageUrl"];
                }
            }

            //create new message, with image
            var msg = context.MakeMessage();
            msg.Text = "Here you go: " + query;
            msg.Attachments = new List<Connector.Attachment>()
            {
                new Connector.Attachment("image/jpeg",imageURL)
            };

            //send message
            await context.PostAsync(msg);
            context.Wait(MessageReceived);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }
    }
}