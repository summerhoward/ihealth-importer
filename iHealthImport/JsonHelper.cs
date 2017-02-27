using System;
using System.Collections.Generic;
using Data.Models;
using Newtonsoft.Json.Linq;

namespace iHealthImport
{
    static class JsonHelper
    {
        private static readonly Dictionary<string, int> LabSourceIds = new Dictionary<string, int>{
            {"steps"                   ,682},
            {"distance"                ,683},
            {"deepSleep"               ,684},
            {"lightSleep"              ,685},
            {"systolic"                ,11},
            {"diastolic"               ,10},
            {"arrhythmia"              ,687},
            {"heartRate"               ,613},
            {"activeCalories"          ,688},
            {"BMI"                     ,16},
            {"bodyFat"                 ,12},
            {"bodyWeight"              ,7},
            {"wake"                    ,686},
            {"fastingGlucose"          ,2},
            {"nonFastingGlucose"       ,15},
        };

        public static List<ImportLabData> ParseLabData(string json)
        {
            var results = new List<ImportLabData>();
            var ihealthObj = JObject.Parse(json);
            JToken ihealthDataObj = null;

            try
            {
                ihealthDataObj = ihealthObj["data"];
            }
            catch
            {
                Console.WriteLine("ERROR: JSON format is incorrect");
            }

            if (ihealthDataObj == null)
                return null;

            foreach (var ihealthUserDataObj in ihealthDataObj)
            {
                foreach (var userDataObj in ihealthUserDataObj)
                {
                    var cardholderIndex = (int?)userDataObj["externalId"];
                    if (cardholderIndex == null || !(cardholderIndex > 0))
                    {
                        continue;
                    }
                    foreach (var dataObj in (JObject)userDataObj)
                    {
                        try
                        {
                            var innerArray = (JArray)dataObj.Value;
                            foreach (var innerData in innerArray)
                            {
                                var obj = (JObject)innerData;
                                foreach (var kvp in obj)
                                {
                                    var value = kvp.Value;
                                    var innerKey = kvp.Key;

                                    if (innerKey == "measuredAt" || innerKey == "mealType" ||
                                        innerKey == "beforeMeal") continue;

                                    var date = (DateTime) obj["measuredAt"];
                                    date = date.AddMilliseconds(-date.Millisecond);
                                    var labData = new ImportLabData
                                    {
                                        LabDate = date,
                                        CardholderIndex = cardholderIndex.Value
                                    };

                                    switch (innerKey)
                                    {
                                        case "bloodGlucose":
                                            innerKey = (bool)obj["beforeMeal"]
                                                ? "fastingGlucose"
                                                : "nonFastingGlucose";
                                            break;
                                        case "bodyWeight":
                                            value = (double)value * 2.20462;
                                            break;
                                        default:
                                            break;
                                    }

                                    labData.LabTypeId = LabSourceIds[innerKey];
                                    labData.Value = value.ToString().ToLower();
                                    results.Add(labData);
                                }
                            }
                        }
                        catch { }

                        try
                        {
                            var obj = (JObject)dataObj.Value;
                            foreach (var kvp in obj)
                            {
                                var value = kvp.Value;
                                var key = kvp.Key;

                                if (key == "measuredAt") continue;

                                var date = (DateTime) obj["measuredAt"];
                                date = date.AddMilliseconds(-date.Millisecond);
                                var labData = new ImportLabData
                                {
                                    LabDate = date,
                                    CardholderIndex = cardholderIndex.Value
                                };

                                switch (key)
                                {
                                    case "distance":
                                        value = (double)value * 0.621371;
                                        break;
                                    default:
                                        break;
                                }

                                labData.LabTypeId = LabSourceIds[key];
                                labData.Value = value.ToString().ToLower();
                                results.Add(labData);
                            }
                        }
                        catch { }
                    }
                }

            }
            return results;
        }
    }
}
