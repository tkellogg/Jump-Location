using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            // This is so that we can read config settings from DLL config file
            string configFile = Assembly.GetExecutingAssembly().Location + ".config";
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile);
            ConfigurationManager.RefreshSection("appSettings");

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
                    home = home ?? Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE"), Environment.GetEnvironmentVariable("HOMEPATH"));
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

        public void AddRecord(IRecord record)
        {
            database.Add(record);
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
            List<string> normalizedSearchTerms = new List<string>();
            foreach (var term in searchTerms)
            {
                normalizedSearchTerms.AddRange(term.Split('\\'));
            }
            return GetMatchesForNormalizedSearchTerm(normalizedSearchTerms);
        }

        private IEnumerable<IRecord> GetMatchesForNormalizedSearchTerm(List<string> searchTerms)
        {
            ReloadIfNecessary();
            var matches = new List<IRecord>();
            // Hack to return everything on empty search query.
            if (!searchTerms.Any())
            {
                searchTerms.Add("");
            }
            for (var i = 0; i < searchTerms.Count(); i++)
            {
                var isLast = i == searchTerms.Count()-1;
                var newMatches = GetMatchesForSingleSearchTerm(searchTerms[i], isLast);
                matches = i == 0 ? newMatches.ToList() : matches.Intersect(newMatches).ToList();
            }

            return matches;
        }

        private IEnumerable<IRecord> GetMatchesForSingleSearchTerm(string search, bool isLast)
        {
            var used = new HashSet<string>();
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

        public IEnumerable<IRecord> GetOrderedRecords(bool includeAll = false)
        {
            return from record in database.Records
                   where record.Weight >= 0 || includeAll
                   orderby record.Weight descending
                   select record;
        }

        public bool RemoveRecord(IRecord record)
        {
            return database.Remove(record);
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
