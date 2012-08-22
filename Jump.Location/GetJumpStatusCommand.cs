using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
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

        protected override void ProcessRecord()
        {
            if (Query == null || Query.Length == 0) 
                ProcessSearch(Controller.GetOrderedRecords());
            else 
                ProcessSearch(Controller.GetMatchesForSearchTerm(Query));
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
