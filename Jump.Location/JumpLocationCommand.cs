using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
    [Cmdlet("Jump", "Location", DefaultParameterSetName = "Query")]
    public class JumpLocationCommand : PSCmdlet
    {
        private static bool _hasRegisteredDirectoryHook;
        private static readonly CommandController Controller = CommandController.DefaultInstance;

        public static IEnumerable<string> GetTabExpansion(string searchTerm)
        {
            return Controller.GetMatchesForSearchTerm(searchTerm).Select(GetResultPath);
        }

        private static string GetResultPath(IRecord record)
        {
            var candidate = record.Path;
            if (candidate.Contains(" "))
                return string.Format("\"{0}\"", candidate);
            return candidate;
        }

        /*
         * x1. Save manipulated jumpstat values
         * x2. -Query switch for returning first string
         * 3. Local search. `j . blah` will only match dirs under cwd. Using `.` will also search outside the DB.
         * 4. -Purge (not terribly high priority)
         * 5. Better PS documentation
         * x6. jumpstat -First, for returning just the first
         * x7. Negative Weight removes from the gene pool
         */

        [Parameter(ParameterSetName = "Query", ValueFromRemainingArguments = true)]
        public string[] Query { get; set; }

        [Parameter(ParameterSetName = "Initialize", 
            HelpMessage = "Initialize Jump-Location by starting to listen to directory changes.")]
        public SwitchParameter Initialize { get; set; }

        [Parameter(ParameterSetName = "Query", HelpMessage = "Use pushd instead of cd to change directory. Same as `pushj`")]
        public SwitchParameter Push { get; set; }

        public static void UpdateTime(string location)
        {
            Controller.UpdateLocation(location);
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (_hasRegisteredDirectoryHook) return;

            InvokeCommand.InvokeScript(@"Set-PSBreakpoint -Variable pwd -Mode Write -Action {
                [Jump.Location.JumpLocationCommand]::UpdateTime($($(Get-Item -Path $(Get-Location))).PSPath);
            }");

            _hasRegisteredDirectoryHook = true;
        }
        
        protected override void ProcessRecord()
        {
            // This lets us do just `Jump-Location` to initialize everything in the profile script
            if (Initialize)
            {
                InvokeCommand.InvokeScript(@"
                    [Jump.Location.JumpLocationCommand]::UpdateTime($($(Get-Item -Path $(Get-Location))).PSPath);
                ");
                return;
            }

            if (Query == null) return;

            // If it has a \ it's probably a full path, so just process it
            if (Query.Length == 1 && Query.First().Contains('\\'))
            {
                ChangeDirectory(Query.First());
                return;
            }

            var best = Controller.FindBest(Query);
            if (best == null) throw new LocationNotFoundException(Query.First());

            var fullPath = GetResultPath(best);
            ChangeDirectory(fullPath);
        }

        private void ChangeDirectory(string fullPath)
        {
            var verb = Push ? "Push" : "Set";
            InvokeCommand.InvokeScript(string.Format("{1}-Location {0}", fullPath, verb));
        }
    }
}
