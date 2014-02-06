namespace Jump.Location
{
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;

    [Cmdlet("Jump", "Location", DefaultParameterSetName = "Query")]
    public class JumpLocationCommand : PSCmdlet
    {
        private static readonly CommandController Controller = CommandController.DefaultInstance;

        public static IEnumerable<string> GetTabExpansion(string line, string lastWord)
        {
            // line is something like "j term1 term2 temr3". 
            // Skip cmdlet name and call match for the rest.
            string[] searchTerms = line.Split().Skip(1).ToArray();
            return Controller.GetMatchesForSearchTerm(searchTerms).Select(GetResultPath);
        }

        private static string GetResultPath(IRecord record)
        {
            var candidate = record.Path;
            if (candidate.Contains(" "))
                return string.Format("\"{0}\"", candidate);
            return candidate;
        }

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

            InvokeCommand.InvokeScript(@"Set-PSBreakpoint -Variable pwd -Mode Write -Action {
                [Jump.Location.JumpLocationCommand]::UpdateTime($($(Get-Item -Path $(Get-Location))).PSPath);
            }");
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

            if (Query == null) { Query = new string[] {}; }

            // If last term is absolute path it's probably because of autocomplition
            // so and we can safely process it here.
            if (Query.Any() && Path.IsPathRooted(Query.Last()))
            {
                ChangeDirectory(Query.Last());
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
            InvokeCommand.InvokeScript(string.Format("{1}-Location \"{0}\"", fullPath, verb));
        }
    }
}
