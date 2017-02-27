using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Models;

namespace Data
{
    public class HealthRepository
    {
        private readonly HealthContextContainer _context = new HealthContextContainer();
        
        public void DoImport(List<ImportLabData> labDataList)
        {
            Console.WriteLine(labDataList.Count() + " ready to be inserted.");

            var labDataToSave = labDataList.Select(x => new LabData
            {
                CardholderIndex = x.CardholderIndex,
                LabTypeId = x.LabTypeId,
                Source = x.Source,
                LabSourceId = x.LabSourceId,
                Value = x.Value,
                LabDate = x.LabDate,
                UserId = x.UserId,
                DateAdded = x.DateAdded
            });

            var startDate = labDataList.OrderBy(x => x.LabDate).Select(x => x.LabDate.Date).FirstOrDefault();
            var endDate = labDataList.OrderByDescending(x => x.LabDate).Select(x => x.LabDate.Date.AddDays(1)).FirstOrDefault();
            var existingLabData = _context.LabDatas.Where(x => x.LabDate >= startDate && x.LabDate <= endDate && x.LabSourceId == 15 && x.Source == 2).ToList();

            foreach (var labData in labDataToSave)
            {
                var dupe = existingLabData.Any(x =>
                    labData.CardholderIndex == x.CardholderIndex && 
                    labData.LabDate == x.LabDate &&
                    labData.LabTypeId == x.LabTypeId &&
                    labData.Value == x.Value
                    );

                if (dupe)
                {
                    Console.WriteLine("Duplicate detected: Cardholder Index:" + labData.CardholderIndex + " Lab Date:" +
                                      labData.LabDate.ToShortDateString() + " Lab Type:" + labData.LabTypeId);
                }
                else
                {
                    try
                    {
                        _context.LabDatas.Add(labData);
                        _context.SaveChanges();
                        Console.WriteLine("Successfully saved: Cardholder Index:" + labData.CardholderIndex + " Lab Date:" +
                                      labData.LabDate.ToShortDateString() + " Lab Type:" + labData.LabTypeId);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Failed to save: Cardholder Index:" + labData.CardholderIndex + " Lab Date:" +
                                      labData.LabDate.ToShortDateString() + " Lab Type:" + labData.LabTypeId);

                        var oldOut = Console.Out;
                        var filestream = new FileStream("FailedImports.csv", FileMode.Append, FileAccess.Write);
                        var streamwriter = new StreamWriter(filestream);
                        Console.SetOut(streamwriter);
                        Console.WriteLine(labData.CardholderIndex + "," + labData.LabDate.ToString("G") + "," + labData.LabTypeId + "," + labData.Value);
                        streamwriter.Close();
                        filestream.Close();
                        Console.SetOut(oldOut);
                    }
                }
            }
        }


    }

}
