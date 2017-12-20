using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ScreenshotApi
{

    public static class NexmoAPIHelper
    {

        public static NexmoSMSResponse SendSMS(string to, string text)
        {
            var wc = new WebClient() { BaseAddress = "https://rest.nexmo.com/sms/json" };
            wc.QueryString.Add("api_key", HttpUtility.UrlEncode(ConfigurationManager.AppSettings["Nexmo.api_key"]));
            wc.QueryString.Add("api_secret", HttpUtility.UrlEncode(ConfigurationManager.AppSettings["Nexmo.api_secret"]));
            wc.QueryString.Add("from", HttpUtility.UrlEncode("Telescope"));
            wc.QueryString.Add("to", HttpUtility.UrlEncode(to));
            wc.QueryString.Add("text", HttpUtility.UrlEncode(text));
            return JsonConvert.DeserializeObject<NexmoSMSResponse>(wc.DownloadString(""));
        }

        public static NexmoTTSResponse TextToSpeech(string to, string text)
        {
            var wc = new WebClient() { BaseAddress = "https://api.nexmo.com/tts/json" };
            wc.QueryString.Add("api_key", HttpUtility.UrlEncode(ConfigurationManager.AppSettings["Nexmo.api_key"]));
            wc.QueryString.Add("api_secret", HttpUtility.UrlEncode(ConfigurationManager.AppSettings["Nexmo.api_secret"]));
            wc.QueryString.Add("from", HttpUtility.UrlEncode("Telescope"));
            wc.QueryString.Add("to", HttpUtility.UrlEncode(to));
            wc.QueryString.Add("text", HttpUtility.UrlEncode(text));
            return JsonConvert.DeserializeObject<NexmoTTSResponse>(wc.DownloadString(""));
        }

        private static NexmoTTSResponse ParseSmsResponseJson(string json)
        {
            json = json.Replace("-", "");  // hyphens are not allowed in in .NET var names
            return JsonConvert.DeserializeObject<NexmoTTSResponse>(json);
        }
    }


    public class NexmoTTSResponse
    {
        public string CallId { get; set; }
        public string To { get; set; }
        public string Status { get; set; }
        public string ErrorText { get; set; }
    }

    public class NexmoSMSResponse
    {
        public string Messagecount { get; set; }
        public List<NexmoMessageStatus> Messages { get; set; }
    }


    public class NexmoMessageStatus
    {
        public string MessageId { get; set; }
        public string To { get; set; }
        public string clientRef;
        public string Status { get; set; }
        public string ErrorText { get; set; }
        public string RemainingBalance { get; set; }
        public string MessagePrice { get; set; }
        public string Network;
    }

    class Program
    {

        static void Main(string[] args)
        {
            var failedCount = 0;
            Ping ping = new Ping();
            while (true)
            {
                if (ping.Send(ConfigurationManager.AppSettings["Machine.IP"]).Status != IPStatus.Success)
                {
                    if (failedCount++ == 3)
                    {
                        NexmoAPIHelper.TextToSpeech(ConfigurationManager.AppSettings["Nexmo.recipient"], "Alert");
                        failedCount = 0;
                        Console.WriteLine("Ping failed 3 times - call initiated. Sleep 10min.");
                        Thread.Sleep(600000);
                    }
                    else
                    {
                        Console.WriteLine("Ping failed - retry " + failedCount);
                    }
                }
                else
                {
                    failedCount = 0;
                    Console.WriteLine("Ping successful, sleep 10s");
                    Thread.Sleep(10000);
                }
            }
        }
    }
}
