using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Data.Models;

namespace iHealthFileImporter
{
    class Program
    {
        private static readonly string RootPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly HealthRepository HealthRepo = new HealthRepository();

        static void Main(string[] args)
        {
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
                Console.WriteLine("Welcome to the iHealth Automated File Importer");
                DoFileCheck();
                Console.WriteLine("Import complete");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                streamwriter.Close();
                filestream.Close();
            }
        }

        private static void DoFileCheck()
        {
            Console.WriteLine("Checking unprocessed files in " + RootPath + "Import");

            Directory.CreateDirectory(RootPath + "Import");
            Directory.CreateDirectory(RootPath + "Processed");

            var files = Directory.GetFiles(RootPath + "Import" + "\\", "*.csv");

            if (files.Length == 0)
            {
                Console.WriteLine("No files found");
                return;
            }

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                try
                {
                    Console.WriteLine("Starting data import by file:" + fileName);
                    DoFileImport(filePath);

                    Console.WriteLine("Moving processed files to " + RootPath + "Processed");
                    File.Move(filePath, RootPath + "Processed" + "\\" + fileName + ".csv");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void DoFileImport(string filePath)
        {
            var labDataList = (from line in File.ReadAllLines(filePath).Skip(1)
                               let columns = line.Split(',')
                               select new ImportLabData
                               {
                                   CardholderIndex = int.Parse(columns[0]),
                                   LabDate = DateTime.Parse(columns[1]),
                                   LabTypeId = int.Parse(columns[2]),
                                   Value = columns[3].ToLower(),
                               }).ToList();
            
            HealthRepo.DoImport(labDataList);
        }
    }
}
