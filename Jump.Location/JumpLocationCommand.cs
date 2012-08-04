using System;
using System.Linq;
using System.Management.Automation;

namespace Jump.Location
{
    [Cmdlet("Jump", "Location")]
    public class JumpLocationCommand : PSCmdlet
    {
        private static bool _hasRegisteredDirectoryHook;
        private static readonly CommandController Controller = new CommandController(@"C:\jump-location.db");

        /*
         * 1. Figure out how long they stay in the directory
         * 2. Log occurences of filename / weight
         * 3. Implement exact substring matches - jump to longest common substring
         * 4. 
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
            var objs = InvokeCommand.InvokeScript(string.Format("Get-Item -Path {0}", Directory));
            var property = objs.First().Properties.First(x => x.Name == "PSPath");
            var fullPath = property.Value.ToString().Split(new[]{"::"}, StringSplitOptions.None)[1];
            
            InvokeCommand.InvokeScript(string.Format("Set-Location {0}", fullPath));
        }
    }
}
