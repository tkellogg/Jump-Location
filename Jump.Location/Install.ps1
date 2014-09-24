# Adapted from posh-git's install.ps1
# Copyright (c) 2010-2011 Keith Dahlby and contributors

param(
	[switch]$WhatIf = $false,
	[string] $WhichProfile = $PROFILE.CurrentUserCurrentHost
)

if($PSVersionTable.PSVersion.Major -lt 3) {
    Write-Warning "Jump-Location requires PowerShell 3.0 or better; you have version $($Host.Version)."
    return
}

if(!(Test-Path $WhichProfile)) {
    Write-Host "Creating PowerShell profile...`n$WhichProfile"
    New-Item $WhichProfile -Force -Type File -ErrorAction Stop -WhatIf:$WhatIf > $null
}

$installDir = Split-Path $MyInvocation.MyCommand.Path -Parent

# Adapted from http://www.west-wind.com/Weblog/posts/197245.aspx
function Get-FileEncoding($Path) {
    $bytes = [byte[]](Get-Content $Path -Encoding byte -ReadCount 4 -TotalCount 4)

    if(!$bytes) { return 'utf8' }

    switch -regex ('{0:x2}{1:x2}{2:x2}{3:x2}' -f $bytes[0],$bytes[1],$bytes[2],$bytes[3]) {
        '^efbbbf'   { return 'utf8' }
        '^2b2f76'   { return 'utf7' }
        '^fffe'     { return 'unicode' }
        '^feff'     { return 'bigendianunicode' }
        '^0000feff' { return 'utf32' }
        default     { return 'ascii' }
    }
}

$profileLine = "Import-Module '$installDir\Jump.Location.psd1'"
if(Select-String -Path $WhichProfile -Pattern $profileLine -Quiet -SimpleMatch) {
    Write-Host "It seems Jump-Location is already installed..."
    return
}

Write-Host "Adding Jump-Location to profile..."
@"

# Load Jump-Location profile
$profileLine

"@ | Out-File $WhichProfile -Append -WhatIf:$WhatIf -Encoding (Get-FileEncoding $WhichProfile)

Write-Host 'Jump-Location sucessfully installed!'
Write-Host 'Please reload your profile for the changes to take effect:'
Write-Host "    . $WhichProfile"