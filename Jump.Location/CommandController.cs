using System;
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
    }
}
