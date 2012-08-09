using System;
using System.IO;
using System.Linq;

namespace Jump.Location
{
    public interface IFileStoreProvider
    {
        void Save(IDatabase database);
        IDatabase Revive();
        DateTime LastChangedDate { get; }
    }

    public class FileStoreUpdatedEventArgs : EventArgs
    {
        public FileStoreUpdatedEventArgs(IDatabase database)
        {
            UpdatedDatabase = database;
        }

        public IDatabase UpdatedDatabase { get; private set; }
    }

    public delegate void FileStoreUpdated(object sender, FileStoreUpdatedEventArgs args);

    class FileStoreProvider : IFileStoreProvider
    {
        private readonly string path;

        public FileStoreProvider(string path)
        {
            this.path = path;
        }

        public event FileStoreUpdated FileStoreUpdated;

        private void OnFileStoreUpdated(object sender, FileStoreUpdatedEventArgs args)
        {
            if (FileStoreUpdated != null)
                FileStoreUpdated(sender, args);
        }

        public void Save(IDatabase database)
        {
            var lines = database.Records.Select(record => 
                string.Format("{1}\t{0}", record.FullName, record.Weight));
            File.WriteAllLines(path, lines.ToArray());
        }

        public IDatabase Revive()
        {
            var db = new Database();
            var allLines = File.ReadAllLines(path);
            var nonBlankLines = allLines.Where(line => !string.IsNullOrEmpty(line) && line.Trim() != string.Empty);
            foreach (var columns in nonBlankLines.Select(line => line.Split('\t')))
            {
                if (columns == null || columns.Length != 2)
                    throw new InvalidOperationException("Row of file didn't have 2 columns separated by a tab");

                var weight = decimal.Parse(columns[0]);
                var record = new Record(columns[1], weight);
                db.Add(record);
            }
            return db;
        }

        public DateTime LastChangedDate { get { return File.GetLastWriteTime(path); } }
    }
}
