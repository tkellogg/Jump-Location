using System;
using System.IO;
using System.Linq;

namespace Jump.Location
{
    public interface IFileStoreProvider
    {
        void Save(IDatabase database);
        IDatabase Revive();
    }

    class FileStoreProvider : IFileStoreProvider
    {
        private readonly string path;

        public FileStoreProvider(string path)
        {
            this.path = path;
        }

        public void Save(IDatabase database)
        {
            
        }

        public IDatabase Revive()
        {
            var db = new Database();
            var allLines = File.ReadAllLines(path);
            foreach (var columns in allLines.Select(line => line.Split('\t')))
            {
                if (columns == null || columns.Length != 2)
                    throw new InvalidOperationException("Row of file didn't have 2 columns separated by a tab");

                var weight = decimal.Parse(columns[1]);
                var record = new Record(columns[0], weight);
                db.Add(record);
            }
            return db;
        }
    }
}
