$name   = "Jump-Location"
$url    = "http://sourceforge.net/projects/jumplocation/files/Jump-Location-0.6.0.zip/download"
$unzipLocation = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

Install-ChocolateyZipPackage $name $url $unzipLocation

$installer = Join-Path $unziplocation 'Install.ps1'
& $installer