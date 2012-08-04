using System.Collections.Generic;
using System.Linq;

namespace Jump.Location
{
    public interface IDatabase
    {
        IEnumerable<IRecord> Records { get; }
        void Add(string fullPath);
        void Add(IRecord record);
        IRecord GetByFullName(string fullName);
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

        public IRecord GetByFullName(string fullName)
        {
            var record = records.FirstOrDefault(x => x.FullName == fullName);

            if (record == null)
            {
                record = new Record(fullName);
                Add(record);
            }

            return record;
        }
    }
}
