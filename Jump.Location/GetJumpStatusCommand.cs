using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
    using System.IO;

    [Cmdlet("Get", "JumpStatus", DefaultParameterSetName = "Query")]
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

        protected override void ProcessRecord()
        {
            
            if (Cleanup.IsPresent)
            {
                DoCleanup();
                return;    
            }

            if (All || Query == null || Query.Length == 0) 
                ProcessSearch(Controller.GetOrderedRecords(All));
            else 
                ProcessSearch(Controller.GetMatchesForSearchTerm(Query));
        }

        private const string FileSystemProvider = @"Microsoft.PowerShell.Core\FileSystem";
        private void DoCleanup()
        {
            bool needsToSave = false;
            foreach (IRecord record in Controller.GetOrderedRecords(true))
            {
                if (record.Provider == FileSystemProvider && !Directory.Exists(record.Path))
                {
                    Controller.RemoveRecord(record);
                    needsToSave = true;
                }
            }
            if (needsToSave)
            {
                Controller.Save();
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
                WriteObject(record);
        }
    }
}
