using System.Management.Automation;

namespace Jump.Location
{
    [Cmdlet("Jump", "Location")]
    public class JumpLocationCommand : PSCmdlet
    {
        private static bool _hasRegisteredDirectoryHook;
        private static readonly CommandController Controller = CommandController.Create(@"C:\Users\Kerianne\jump-location.txt");

        /*
         * x1. Figure out how long they stay in the directory
         * x2. Log occurences of filename / weight
         * x3. Tail matches - search matches beginning of last segment of path
         * x4. Make MSI installer for easy use
         * 5. Weighting algorithm - match what Autojump does to increase weights
         * 6. Match what Autojump does to degrade weights
         * 7. Multiple args - last arg is a tail match, previous args match previous segments
         * 8. Tab completion - list 5 best matches
         * x9. Get-JumpStat
         */

        [Parameter(Position = 0)]
        public string Directory { get; set; }

        [Parameter]
        public bool Status { get; set; }

        [Parameter]
        public bool Initialize { get; set; }

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
            if (Status)
            {
                Controller.PrintStatus();
                return;
            }

            // This lets us do just `Jump-Location` to initialize everything in the profile script
            if (Initialize)
            {
                InvokeCommand.InvokeScript(@"
                    [Jump.Location.JumpLocationCommand]::UpdateTime($($(Get-Item -Path $(Get-Location))).PSPath);
                ");
                return;
            }

            if (Directory == null) return;

            var best = Controller.FindBest(Directory);
            if (best == null) throw new LocationNotFoundException(Directory);

            var fullPath = best.Path;
            InvokeCommand.InvokeScript(string.Format("Set-Location {0}", fullPath));
        }
    }
}
