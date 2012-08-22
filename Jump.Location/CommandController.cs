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
        private static CommandController defaultInstance;

        internal CommandController(IDatabase database, IFileStoreProvider fileStore)
        {
            this.database = database;
            this.fileStore = fileStore;
            var thread = new Thread(SaveLoop);
            thread.Start();
        }

        public static CommandController DefaultInstance
        {
            get
            {
                if (defaultInstance == null)
                {
                    var home = Environment.GetEnvironmentVariable("USERPROFILE");
                    // TODO: I think there's potential here for a bug
                    home = home ?? @"C:\";
                    var dbLocation = Path.Combine(home, "jump-location.txt");
                    defaultInstance = Create(dbLocation);
                }
                return defaultInstance;
            }
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

        public void Save()
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
                else Thread.Sleep(10);
            }
        }

        private void ReloadIfNecessary()
        {
            if (fileStore.LastChangedDate <= lastSaveDate) return;
            database = fileStore.Revive();
            lastSaveDate = DateTime.Now;
        }

        public IRecord FindBest(params string[] search)
        {
            return GetMatchesForSearchTerm(search).FirstOrDefault();
        }

        public IEnumerable<IRecord> GetMatchesForSearchTerm(params string[] searchTerms)
        {
            ReloadIfNecessary();
            var used = new HashSet<string>();

            var matches = new List<IRecord>();
            for (var i = 0; i < searchTerms.Length; i++)
            {
                var isLast = i == searchTerms.Length - 1;
                var newMatches = GetMatchesForSingleSearchTerm(searchTerms[i], used, isLast);
                matches = i == 0 ? newMatches.ToList() : matches.Intersect(newMatches).ToList();
            }

            return matches;
        }

        private IEnumerable<IRecord> GetMatchesForSingleSearchTerm(string search, HashSet<string> used, bool isLast)
        {
            search = search.ToLower();
            foreach (var record in GetOrderedRecords()
                    .Where(x => x.PathSegments.Last().StartsWith(search)))
            {
                used.Add(record.Path);
                yield return record;
            }

            foreach (var record in GetOrderedRecords()
                    .Where(record => !used.Contains(record.Path))
                    .Where(x => x.PathSegments.Last().Contains(search)))
            {
                used.Add(record.Path);
                yield return record;
            }

            if (isLast) yield break;

            foreach (var record in GetOrderedRecords()
                    .Where(record => !used.Contains(record.Path))
                    .Where(x => x.PathSegments.Any(s => s.StartsWith(search))))
            {
                used.Add(record.Path);
                yield return record;
            }

            foreach (var record in GetOrderedRecords()
                    .Where(record => !used.Contains(record.Path))
                    .Where(x => x.PathSegments.Any(s => s.Contains(search))))
            {
                used.Add(record.Path);
                yield return record;
            }
        }

        public IEnumerable<IRecord> GetOrderedRecords()
        {
            return from record in database.Records
                   where record.Weight >= 0
                   orderby record.Weight descending
                   select record;
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
