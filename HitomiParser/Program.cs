using System;
using System.IO;
using System.Net;
using System.Threading;

using Newtonsoft.Json;

namespace HitomiParser
{
    enum LogType
    {
        INFO, WARN, CRIT
    }

    class Program
    {
        const string HITOMI_PREFIX = "https://ltn.hitomi.la/";
        const int DBJSONCOUNT = 20;
        static int CurrentJsonCount = 0;

        static void Log(LogType type, string message)
        {
            Console.WriteLine($"[{type}] {DateTime.Now} {message} ");
        }

        static void Main(string[] args)
        {
            Log(LogType.INFO, "HitomiParser Started, Synchronizing Database...");

            for (int i = 0; i < DBJSONCOUNT; ++i)
            {
                var webclient = new WebClient();
                webclient.DownloadFileCompleted += (obj, e) =>
                {
                    CurrentJsonCount++;
                };
                webclient.DownloadFileAsync(new Uri($"{HITOMI_PREFIX}galleries{i}.json"), $"galleries{i}.json");
            }

            while (CurrentJsonCount != DBJSONCOUNT) { Thread.Sleep(100); }

            Log(LogType.INFO, "Database Synchronization Successful, Creating Result File...");
            var file = File.Create($"HPRESULT.txt");
            var filer = new StreamWriter(file);
            
            for (int i = 0; i < DBJSONCOUNT; ++i)
            {
                Log(LogType.INFO, $"Parsing: galleries{i}.json");

                var jsonRead = File.ReadAllText($"galleries{i}.json");
                var jsonParseObject = JsonConvert.DeserializeObject<dynamic>(jsonRead);
                foreach (dynamic e in jsonParseObject)
                {
                    if (e.l == "japanese") 
                    {
                        filer.WriteLine($"{e.id}");
                    }
                }
            }

            filer.Close();
            file.Close();

            Log(LogType.INFO, $"ALL DONE!");
            Console.ReadKey();
            return;
        }
    }
}
