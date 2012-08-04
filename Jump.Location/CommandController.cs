﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Jump.Location
{
    class CommandController
    {
        private readonly IDatabase database;
        private readonly IFileStoreProvider fileStore;
        private bool needsToSave;
        private DirectoryWaitPeriod waitPeriod;

        public CommandController(IDatabase database, IFileStoreProvider fileStore)
        {
            this.database = database;
            this.fileStore = fileStore;
            var thread = new Thread(SaveLoop);
            thread.Start();
        }

        public CommandController(string path)
            :this(new Database(), new FileStoreProvider(path))
        {
        }

        public void UpdateLocation(string fullName)
        {
            var record = database.GetByFullName(fullName);
            var dontSave = waitPeriod == null;
            waitPeriod = new DirectoryWaitPeriod(record, DateTime.Now);

            if (dontSave) return;

            waitPeriod.CloseAndUpdate();
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
                    }
                    catch(Exception e)
                    {
                        Console.Error.WriteLine("Jump-Location received {0}: {1}", e.GetType().Name, e.Message);
                    }
                }
                else Thread.Sleep(0);
            }
        }

        public IRecord FindBest(string search)
        {
            return GetMatchesForSearchTerm(search).FirstOrDefault();
        }

        internal IEnumerable<IRecord> GetMatchesForSearchTerm(string search)
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
    }
}
