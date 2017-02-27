using System;

namespace Data.Models
{
    public class ImportLabData : LabData
    {
        public ImportLabData()
        {
            LabSourceId = 15;
            Source = 2;
            UserId = 1003;
            DateAdded = DateTime.Now;
        }
    }
}
