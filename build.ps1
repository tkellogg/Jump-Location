
if (-not $(Test-Path Build)) {
	mkdir Build
}

cp Readme.md Build
cp License.md Build
cp Jump.Location\bin\Debug\Jump.Location.dll Build
cp Jump.Location\bin\Debug\Jump.Location.psd1 Build
cp Jump.Location\bin\Debug\Jump.Location.Format.ps1xml Build
cp Jump.Location\bin\Debug\Types.ps1xml Build
cp Jump.Location\bin\Debug\Install.ps1 Build
cp Jump.Location\bin\Debug\Load.ps1 Build
cp Jump.Location\bin\Debug\TabExpansion.ps1 Build
