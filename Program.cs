using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("your_token");

        static void Main(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;

                if (message == null || message.Type != MessageType.Text) return;

                var webClient = new System.Net.WebClient();

                webClient.Encoding = Encoding.UTF8;

                switch (message.Text)
                {
                    case "/start":
                        {
                            ReplyKeyboardMarkup ReplyKeyboard = new[] { new[] { "Exchange Rate", "Crypto" } };

                            await Bot.SendTextMessageAsync(message.Chat.Id, "Commands", replyMarkup: ReplyKeyboard);

                            break;
                        }

                    case "Exchange Rate":
                        {
                            string HTML = webClient.DownloadString(@"https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange");

                            XDocument doc = XDocument.Parse(HTML);

                            string result = "Results ";

                            foreach (var item in doc.Element("exchange").Elements("currency"))
                            {
                                if (item.Element("txt").Value.Equals("Долар США"))
                                {
                                    result += String.Format("for {1}:\n\n1 USD => {0} UAH\n", ReturnData(item), item.Element("exchangedate").Value.ToString());
                                }
                                else if (item.Element("txt").Value.Equals("Євро"))
                                {
                                    result += String.Format("1 EUR => {0} UAH\n", ReturnData(item));
                                }
                                else if (item.Element("txt").Value.Equals("Злотий"))
                                {
                                    result += String.Format("1 PLN => {0} UAH\n", ReturnData(item));
                                }
                            }

                            await Bot.SendTextMessageAsync(message.Chat.Id, result);

                            break;
                        }

                    case "Crypto":
                        {
                            string json = webClient.DownloadString(@"http://coincap.io/page/KRB");

                            JObject doc = JObject.Parse(json);

                            string result = "KRB => " + string.Format("0{0:.##}", double.Parse((doc.GetValue("price_usd").ToString()))).Replace(',', '.') + " USD\n";

                            json = webClient.DownloadString(@"http://coincap.io/page/ETH");
                            doc = JObject.Parse(json);
                            result += "ETH => " + string.Format("{0:.##}", double.Parse((doc.GetValue("price_usd").ToString()))).Replace(',', '.') + " USD";

                            await Bot.SendTextMessageAsync(message.Chat.Id, result);

                            break;
                        }

                    default:
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, "I don't understand you");

                            break;
                        }
                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string ReturnData(XElement item) => string.Format("{0:.##}", double.Parse(item.Element("rate").Value.ToString().Replace('.', ','))).Replace(',', '.');
    }
}