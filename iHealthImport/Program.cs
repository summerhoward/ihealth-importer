using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Data;

namespace iHealthImport
{
    static class Program
    {
        private static readonly string RootPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly HealthRepository HealthRepo = new HealthRepository();
        private static readonly string RequestStartDate = ConfigurationManager.AppSettings["RequestStartDate"];

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var requestStartDate = DateTime.Parse(RequestStartDate);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var logDirectory = RootPath + "Log\\";
            var logFileName = logDirectory + DateTime.Now.ToString("MMddyyyyhhmmss") + ".txt";

            Directory.CreateDirectory(logDirectory);
            
            var file = File.Create(logFileName);
            file.Close();
            var filestream = new FileStream(logFileName, FileMode.Append, FileAccess.Write);
            var streamwriter = new StreamWriter(filestream){AutoFlush = true};
            Console.SetOut(streamwriter);
            try
            {
                Console.WriteLine("Welcome to the iHealth Automated API Request Importer");
                var currentRequestDate = requestStartDate;
                var currentDate = DateTime.Now.Date;

                while (currentRequestDate < currentDate)
                {

                    var status = DoApiCheck(currentRequestDate);
                    Console.WriteLine("Import complete");

                    if (status == 1)
                    {
                        Console.WriteLine("Modifying Request Start Date to next day");
                        currentRequestDate = currentRequestDate.AddDays(1);
                        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        config.AppSettings.Settings["RequestStartDate"].Value = currentRequestDate.ToShortDateString();
                        config.Save(ConfigurationSaveMode.Modified);
                    }

                    if (currentRequestDate == currentDate) continue;
                    Console.WriteLine("Sleeping for 1 minute to allow API accept next request");
                    Thread.Sleep((int) TimeSpan.FromMinutes(1).TotalMilliseconds);
                }
                Console.WriteLine("Request Start Date has reached yesterday");

                Console.WriteLine("Resetting Request Start Date to original date");
                {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["RequestStartDate"].Value = requestStartDate.ToShortDateString();
                    config.Save(ConfigurationSaveMode.Modified);
                }
            }
            finally
            {
                streamwriter.Close();
                filestream.Close();
            }
        }



        private static int DoApiCheck(DateTime date)
        {
            Console.WriteLine("Requesting Data for " + date.ToShortDateString());
            var apiUri = "/v1/vitals/all";
            var uri = "https://aph.ihealthconnect.com" + apiUri + "?date=" + date.Year + date.Month.ToString("D2") + date.Day.ToString("D2");

            var request = WebRequest.CreateHttp(uri);

            var token = new Authentication().CreateToken(apiUri, "Nkl8OfOu2--E1xHx8hZvinxR6bXT_rtZDTm43KqGGFt");

            request.Accept = "application/json";
            request.Headers = new WebHeaderCollection
            {
                {"X-Api-Key", "XYTLY6QxdxMf5SeiK"},
                {"X-Api-Signature", token},
                //{HttpRequestHeader.AcceptEncoding, "gzip"}
            };
            
            string json = null;
            try
            {
                Console.WriteLine("Sending rquest: " + uri);
                var response = request.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch
            {
                Console.WriteLine("ERROR: Falied to get response");
                return 0;
            }

            if (!string.IsNullOrEmpty(json))
            {
                Console.WriteLine("Parsing response");
                var labDataResults = JsonHelper.ParseLabData(json);

                Console.WriteLine("Starting data import by response");
                HealthRepo.DoImport(labDataResults);
            }
            else
            {
                Console.WriteLine("No data to import");
            }
            return 1;
        }

        public class Authentication
        {
            public string CreateToken(string message, string secret)
            {
                byte[] keyByte = Encoding.UTF8.GetBytes(secret);
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                using (var hmacsha256 = new HMACSHA256(keyByte))
                {
                    var hashmessage = hmacsha256.ComputeHash(messageBytes);
                    string hex = BitConverter.ToString(hashmessage);
                    return hex.Replace("-", "").ToLower();
                }
            }
        }
    }
}
