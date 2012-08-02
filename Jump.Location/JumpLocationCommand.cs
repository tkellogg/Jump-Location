using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace Jump.Location
{
    [Cmdlet("Jump", "Location")]
    public class JumpLocationCommand : PSCmdlet
    {
        [Parameter(Position = 0)]
        public string Directory { get; set; }
        
        protected override void ProcessRecord()
        {
            var objs = InvokeCommand.InvokeScript(string.Format("Get-Item -Path {0}", Directory));
            
            InvokeCommand.InvokeScript(string.Format("Set-Location {0}", Directory));
        }
    }
}
