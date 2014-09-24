<#  
.SYNOPSIS  
    copy files from bin folder to Build folder
#>

if (-not $(Test-Path Build)) {
	$null = mkdir Build
}

$source = "Jump.Location\bin\Debug"
$destination = "Build"

Write-Host "Copy bits from $source to $destination"

cp Readme.md $destination
cp License.md $destination

cp $source\Jump.Location.dll $destination
cp $source\Jump.Location.psd1 $destination
cp $source\Jump.Location.Format.ps1xml $destination
cp $source\Types.ps1xml $destination
cp $source\Install.ps1 $destination
cp $source\Load.ps1 $destination
cp $source\TabExpansion.ps1 $destination
