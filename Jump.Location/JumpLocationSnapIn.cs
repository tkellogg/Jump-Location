using System.ComponentModel;
using System.Management.Automation;

namespace Jump.Location
{
    [RunInstaller(true)]
    public class JumpLocationSnapIn : PSSnapIn
    {
        public override string Name
        {
            get { return "Jump-Location"; }
        }

        public override string Vendor
        {
            get { return "Tim Kellogg"; }
        }

        public override string Description
        {
            get { return "Like Set-Location but reads your mind"; }
        }
    }
}
