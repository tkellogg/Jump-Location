using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
    [Cmdlet("Get", "JumpStatus")]
    public class GetJumpStatusCommand : PSCmdlet
    {
        private static readonly CommandController Controller = CommandController.DefaultInstance;

        [Parameter(ValueFromRemainingArguments = true)]
        public string[] Directory { get; set; }

        [Parameter]
        public SwitchParameter First { get; set; }

        protected override void ProcessRecord()
        {
            if (Directory == null || Directory.Length == 0) 
                ProcessSearch(Controller.GetOrderedRecords());
            else 
                ProcessSearch(Controller.GetMatchesForSearchTerm(Directory));
        }

        private void ProcessSearch(IEnumerable<IRecord> records)
        {
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
