using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
    [Cmdlet("Jump", "Location")]
    public class JumpLocationCommand : PSCmdlet
    {
        private static bool _hasRegisteredDirectoryHook;
        private static readonly CommandController Controller = CommandController.DefaultInstance;

        public static IEnumerable<string> GetTabExpansion(string searchTerm)
        {
            return Controller.GetMatchesForSearchTerm(searchTerm).Select(x => x.Path);
        }

        /*
         * 1. Save manipulated jumpstat values
         * x2. -Query switch for returning first string
         * 3. Local search. `j . blah` will only match dirs under cwd. Using `.` will also search outside the DB.
         * 4. -Purge (not terribly high priority)
         * 5. Better PS documentation
         * 6. jumpstat -First, for returning just the first
         */

        [Parameter(ValueFromRemainingArguments = true)]
        public string[] Directory { get; set; }

        [Parameter]
        public SwitchParameter Initialize { get; set; }

        [Parameter]
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

            if (Directory == null) return;

            // If it has a \ it's probably a full path, so just process it
            if (Directory.Length == 1 && Directory.First().Contains('\\'))
            {
                ChangeDirectory(Directory.First());
                return;
            }

            var best = Controller.FindBest(Directory);
            if (best == null) throw new LocationNotFoundException(Directory.First());

            var fullPath = best.Path;
            ChangeDirectory(fullPath);
        }

        private void ChangeDirectory(string fullPath)
        {
            var verb = Push ? "Push" : "Set";
            InvokeCommand.InvokeScript(string.Format("{1}-Location {0}", fullPath, verb));
        }
    }
}
