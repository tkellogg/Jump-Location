$version = "0.6.0"
# zip and push binaries to SourceForge first...

.\.nuget\nuget pack "Chocolatey\Jump-Location.nuspec" -Version $version -OutputDirectory Chocolatey
# now upload .nupkg to Chocolatey.org...
# ?
# profit!