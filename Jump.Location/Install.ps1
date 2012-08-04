# Install.ps1
#
# 1. Install Jump-Location
# 2. Setup alias j
# 3. Setup jumpstat alias (function)
# 4. Start watching directory changes

$fullpath = Split-Path -Parent $MyInvocation.MyCommand.Path
$dllpath = $fullpath + "\Jump.Location.dll"

Import-Module $dllpath -Global -DisableNameChecking
New-Alias -Name j -Value Jump-Location -Scope Global

function global:Jumpstat() {
	Jump-Location -Status $true
}

Jump-Location -Initialize $true
