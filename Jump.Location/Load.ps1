# Install.ps1
#
# 1. Install Jump-Location
# 2. Setup alias j
# 3. Setup jumpstat alias (function)
# 4. Start watching directory changes
# 5. Register tab expansion

$fullpath = Split-Path -Parent $MyInvocation.MyCommand.Path
$dllpath = $fullpath + "\Jump.Location.dll"

if (-Not (Get-Command Jump-Location -ErrorAction SilentlyContinue)) {

	Import-Module $dllpath -Global -DisableNameChecking
	New-Alias -Name j -Value Jump-Location -Scope Global

	New-Alias -Name jumpstat -Value Get-JumpStatus -Scope Global

	function global:PushJ {
		Param (
			[Parameter(ValueFromRemainingArguments=$true)] $args
		)
		Jump-Location @args -Push
	}

	function global:getj {
		Param (
			[Parameter(ValueFromRemainingArguments=$true)] $args
		)
		jumpstat -First @args
	}

	function global:xj {
		Param (
			[Parameter(ValueFromRemainingArguments=$true)] $args
		)
		explorer $(jumpstat -First @args)
	}

	Jump-Location -Initialize

	& $($fullpath + "\TabExpansion.ps1")

} else {
	Write-Warning "Jump-Location already loaded"
}
