using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
    using System;
    using System.Configuration;
    using System.IO;

    [Cmdlet(VerbsCommon.Get, "JumpStatus", DefaultParameterSetName = "Query")]
    public class GetJumpStatusCommand : PSCmdlet
    {
        private static readonly CommandController Controller = CommandController.DefaultInstance;

        [Parameter(ParameterSetName = "Query", ValueFromRemainingArguments = true)]
        public string[] Query { get; set; }

        [Parameter(ParameterSetName = "Query", HelpMessage = "Only retrieve the first record matching the query. Same as `getj`")]
        public SwitchParameter First { get; set; }

        [Parameter(ParameterSetName = "Save", HelpMessage = "Saves any pending changes from setting weights on records explicitly.")]
        public SwitchParameter Save { get; set; }

        [Parameter(ParameterSetName = "Query", HelpMessage = "Includes all results, even if the weight is negative.")]
        public SwitchParameter All { get; set; }

        [Parameter(ParameterSetName = "Cleanup", HelpMessage = "Remove obsolete(not existing on the file system) records from DB.")]
        public SwitchParameter Cleanup { get; set; }

        [Parameter(ParameterSetName = "Scan", HelpMessage = "Scan and discover subdirectories.")]
        [AllowEmptyString]
        public string Scan { get; set; }

        protected override void ProcessRecord()
        {

            if (this.ParameterSetName == "Cleanup")
            {
                DoCleanup();
                return;    
            }

            if (this.ParameterSetName == "Scan")
            {
                DoScan();
                return;
            }

            if (All || Query == null || Query.Length == 0)
            {
                ProcessSearch(Controller.GetOrderedRecords(All));
            }
            else
            {
                ProcessSearch(Controller.GetMatchesForSearchTerm(Query));
            }
        }

        private const string FileSystemProvider = @"Microsoft.PowerShell.Core\FileSystem";
        private void DoCleanup()
        {
            int recordsRemoved = 0;
            foreach (IRecord record in Controller.GetOrderedRecords(true))
            {
                if (record.Provider == FileSystemProvider && !Directory.Exists(record.Path))
                {
                    Controller.RemoveRecord(record);
                    recordsRemoved++;
                }
            }
            if (recordsRemoved > 0)
            {
                Controller.Save();
                WriteVerbose("Number of records cleaned: " + recordsRemoved + ".");
            }
        }

        private void DoScan()
        {
            if (string.IsNullOrEmpty(this.Scan))
            {
                // set default to current directory
                this.Scan = ".";
            }
            this.Scan = GetUnresolvedProviderPathFromPSPath(this.Scan);
            WriteVerbose("Discovering new folders.");

            int count = 0;
            foreach (string fullPath in GetChildFoldersRec(this.Scan))
            {
                WriteVerbose(string.Format("[Touching] {0}", fullPath));
                Controller.TouchRecord(string.Format("{0}::{1}", FileSystemProvider, fullPath));
                count++;
            }

            Controller.Save();
            WriteVerbose(string.Format("Number of directories touched: {0}.", count));
        }

        private IEnumerable<string> GetChildFoldersRec(string path)
        {
            yield return path; 
            foreach (string dir in Directory.GetDirectories(path))
            {
                foreach (string childDir in GetChildFoldersRec(dir))
                {
                    yield return childDir;
                }
            }
        }

        private void ProcessSearch(IEnumerable<IRecord> records)
        {
            if (Save)
            {
                Controller.Save();
                return;
            }

            if (First)
            {
                var record = records.FirstOrDefault();
                if (record != null) 
                    WriteObject(record);
                return;
            }

            foreach (var record in records)
            {
                WriteObject(record);
            }
            WriteVerbose("Number of records: " + records.Count() + ".");
        }
    }
}
