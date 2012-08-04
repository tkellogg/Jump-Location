using System.Collections.Generic;

namespace Jump.Location
{
    public interface IDatabase
    {
        IEnumerable<IRecord> Records { get; }
        void Add(string fullPath);
        void Add(IRecord record);
    }

    class Database : IDatabase
    {
        readonly List<IRecord> records = new List<IRecord>();

        public IEnumerable<IRecord> Records { get { return records; } } 

        public void Add(string fullPath)
        {
            records.Add(new Record(fullPath));
        }

        public void Add(IRecord record)
        {
            records.Add(record);
        }
    }
}
