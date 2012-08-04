using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
    [Cmdlet("Jump", "Location")]
    public class JumpLocationCommand : PSCmdlet
    {
        private static bool _hasRegisteredDirectoryHook;
        private static readonly CommandController Controller = new CommandController(@"C:\Users\Kerianne\jump-location.txt");

        /*
         * x1. Figure out how long they stay in the directory
         * x2. Log occurences of filename / weight
         * x3. Tail matches - search matches beginning of last segment of path
         * 4. Weighting algorithm - match what Autojump does to increase weights
         * 5. Match what Autojump does to degrade weights
         * 6. Multiple args - last arg is a tail match, previous args match previous segments
         * 7. Tab completion - list 5 best matches
         */

        [Parameter(Position = 0)]
        public string Directory { get; set; }

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
            }", "hello Jim");

            _hasRegisteredDirectoryHook = true;
        }
        
        protected override void ProcessRecord()
        {
            var best = Controller.FindBest(Directory);
            if (best == null) throw new LocationNotFoundException(Directory);

            var fullPath = best.Path;
            InvokeCommand.InvokeScript(string.Format("Set-Location {0}", fullPath));
        }
    }
}
