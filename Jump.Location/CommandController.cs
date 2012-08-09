using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jump.Location
{
    class CommandController
    {
        private IDatabase database;
        private readonly IFileStoreProvider fileStore;
        private bool needsToSave;
        private DirectoryWaitPeriod waitPeriod;
        private DateTime lastSaveDate = DateTime.Now;

        internal CommandController(IDatabase database, IFileStoreProvider fileStore)
        {
            this.database = database;
            this.fileStore = fileStore;
            var thread = new Thread(SaveLoop);
            thread.Start();
        }

        public static CommandController Create(string path)
        {
            var fileStore = new FileStoreProvider(path);
            var database = File.Exists(path) ? fileStore.Revive() : new Database();
            return new CommandController(database, fileStore);
        }

        public void UpdateLocation(string fullName)
        {
            if (waitPeriod != null)
                waitPeriod.CloseAndUpdate();

            var record = database.GetByFullName(fullName);
            waitPeriod = new DirectoryWaitPeriod(record, DateTime.Now);
            Save();
        }

        private void Save()
        {
            needsToSave = true;
        }

        private void SaveLoop()
        {
            while(true)
            {
                if (needsToSave)
                {
                    try
                    {
                        needsToSave = false;
                        fileStore.Save(database);
                        lastSaveDate = DateTime.Now;
                    }
                    catch(Exception e)
                    {
                        EventLog.WriteEntry("Application", string.Format("{0}\r\n{1}", e, e.StackTrace));
                    }
                }
                else Thread.Sleep(0);
            }
        }

        private void ReloadIfNecessary()
        {
            if (fileStore.LastChangedDate <= lastSaveDate) return;
            database = fileStore.Revive();
            lastSaveDate = DateTime.Now;
        }

        public IRecord FindBest(string search)
        {
            return GetMatchesForSearchTerm(search).FirstOrDefault();
        }

        public IEnumerable<IRecord> GetMatchesForSearchTerm(string search)
        {
            ReloadIfNecessary();
            var used = new HashSet<string>();
            search = search.ToLower();

            foreach (var record in GetOrderedRecords()
                    .Where(x => x.PathSegments.Last().StartsWith(search)))
            {
                used.Add(record.Path);
                yield return record;
            }

            foreach (var record in GetOrderedRecords()
                    .Where(x => x.PathSegments.Last().Contains(search)))
            {
                if (used.Contains(record.Path)) continue;
                used.Add(record.Path);
                yield return record;
            }

            foreach (var record in GetOrderedRecords()
                    .Where(x => x.PathSegments.Any(s => s.StartsWith(search))))
            {
                if (used.Contains(record.Path)) continue;
                used.Add(record.Path);
                yield return record;
            }

            foreach (var record in GetOrderedRecords()
                    .Where(x => x.PathSegments.Any(s => s.Contains(search))))
            {
                if (used.Contains(record.Path)) continue;
                used.Add(record.Path);
                yield return record;
            }
        }

        private IEnumerable<IRecord> GetOrderedRecords()
        {
            return database.Records.OrderByDescending(x => x.Weight);
        }

        public void PrintStatus()
        {
            foreach (var record in GetOrderedRecords())
            {
                Console.WriteLine("{0:   0.00}  {1}", record.Weight, record.Path);
            }
        }
    }
}
