using Microsoft.Owin.Hosting;
using Nexmo.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenshotApi
{
    class Program
    {

        static void Main(string[] args)
        {
            //Web server
            string baseAddress = "http://*:7777/";
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine("Web api running on http://*:7777//api/screenshot/take. Press any key to stop AstroMonitor");

                if (ConfigurationManager.AppSettings["PHD2.Monitor"] == "1")
                {
                    Task.Factory.StartNew(() =>
                    {
                        Console.WriteLine("Starting monitoring...");
                        try
                        {
                            int? _starLostCount = null;
                            int _starLostCountCurr;
                            FileInfo _file = null;
                            FileInfo _fileLatest;
                            while (true)
                            {
                                var directory = new DirectoryInfo(ConfigurationManager.AppSettings["PHD2.Monitor.LogFolder"]);
                                _fileLatest = directory.GetFiles()
                                    .Where(f => Regex.IsMatch(f.Name, @"PHD2_DebugLog_(\d{4})-(\d{2})-(\d{2})_\d{6}\.txt"))
                                    .OrderByDescending(f => f.LastWriteTime)
                                    .First();

                                Console.Write($"{DateTime.Now.ToString("MM.dd.yyyy HH:mm")} Scanning file {_fileLatest.FullName}...");

                                using (var fs = new FileStream(_fileLatest.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                using (var sr = new StreamReader(fs, Encoding.Default))
                                {
                                    _starLostCountCurr = Regex.Matches(sr.ReadToEnd(), "Star lost").Count;
                                }

                                if (_file == null || _file.FullName != _fileLatest.FullName)
                                {
                                    //first time
                                    _starLostCount = _starLostCountCurr;
                                    _file = _fileLatest;
                                    Console.WriteLine($" initial scan finished at {DateTime.Now.ToString("MM.dd.yyyy HH:mm")}, initial star lost count: {_starLostCount} ... sleep 30s");
                                    Thread.Sleep(30000);
                                }
                                else
                                {
                                    if (_starLostCountCurr> (_starLostCount + 10))
                                    {
                                        
                                        Console.WriteLine($"... scan finished at {DateTime.Now.ToString("MM.dd.yyyy HH: mm")} -  !!!!! STAR LOST !!!!! ({_starLostCountCurr} > {_starLostCount})");
                                        var results = SMS.Send(new SMS.SMSRequest
                                        {
                                            from = "Telescope",
                                            to = ConfigurationManager.AppSettings["Nexmo.recipient"],
                                            text = "PHD Alert: star lost"
                                        });

                                        Console.WriteLine($"SMS sent to {ConfigurationManager.AppSettings["Nexmo.recipient"]} ... sleep 10min");
                                        Thread.Sleep(1000 * 60 * 10);
                                        
                                        //reset scan
                                        _file = null;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"... scan finished at {DateTime.Now.ToString("MM.dd.yyyy HH:mm")} - OK ... sleep 30s");
                                        Thread.Sleep(30000);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Monitoring failed: {e.Message} Trace: {e.StackTrace}");
                        }
                    });
                }

                Console.ReadLine();
            }
        }
    }
}
