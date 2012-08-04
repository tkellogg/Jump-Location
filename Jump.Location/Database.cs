using System.Collections.Generic;

namespace Jump.Location
{
    class Database
    {
        readonly List<IRecord> records = new List<IRecord>();

        public IEnumerable<IRecord> Records { get { return records; } } 

        public void AddRecord(string fullPath)
        {
            records.Add(new Record(fullPath));
        }
    }
}
