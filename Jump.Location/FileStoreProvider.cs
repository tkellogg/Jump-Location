using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

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
        private readonly string _path;
        private readonly string _pathTemp;
        private readonly string _mutexId;
        private readonly MutexSecurity _securitySettings;

        private const string TempPrefix = ".tmp";

        public FileStoreProvider(string path)
        {
            this._path = path;
            this._pathTemp = path + TempPrefix;

            // global mutex implementation from http://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c

            // get application GUID as defined in AssemblyInfo.cs
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString(CultureInfo.InvariantCulture);

            // unique id for global mutex - Global prefix means it is global to the machine
            _mutexId = string.Format("Global\\{{{0}}}", appGuid);

            // setting up security for multi-user usage
            // work also on localized systems (don't use just "Everyone") 
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            _securitySettings = new MutexSecurity();
            _securitySettings.AddAccessRule(allowEveryoneRule);
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
            string.Format("{1}\t{0}", record.FullName, record.Weight.ToString(CultureInfo.InvariantCulture)));

            bool createdNew;
            using (var mutex = new Mutex(false, _mutexId, out createdNew, _securitySettings))
            {
                var hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(1000, false);
                        if (hasHandle == false)
                        {
                            // ignore
                            return;
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact the mutex was abandoned in another process, it will still get aquired
                        hasHandle = true;
                    }
                    
                    // We can lose all history, if powershell will be closed during operation.
                    File.WriteAllLines(_pathTemp, lines.ToArray());
                    // NTFS guarantees atomic move operation http://stackoverflow.com/questions/774098/atomicity-of-file-move
                    // So File.Move gurantees atomic, but doesn't support overwrite
                    File.Copy(_pathTemp, _path, true);
                }
                finally
                {
                    // edited by acidzombie24, added if statemnet
                    if(hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }

        public IDatabase Revive()
        {
            var db = new Database();
            var allLines = File.ReadAllLines(_path);
            var nonBlankLines = allLines.Where(line => !string.IsNullOrEmpty(line) && line.Trim() != string.Empty);
            foreach (var columns in nonBlankLines.Select(line => line.Split('\t')))
            {
                if (columns == null || columns.Length != 2)
                    throw new InvalidOperationException("Row of file didn't have 2 columns separated by a tab");

                var weight = 0M;
                decimal.TryParse(columns[0], NumberStyles.Any, CultureInfo.InvariantCulture, out weight);
                var record = new Record(columns[1], weight);
                db.Add(record);
            }
            return db;
        }

        public DateTime LastChangedDate { get { return File.GetLastWriteTime(_path); } }
    }
}
