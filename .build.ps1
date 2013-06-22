<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)

.Description
	How to use this script and build the module:

	Get the utility script Invoke-Build.ps1:
	https://github.com/nightroman/Invoke-Build

	Copy it to the path. Set location to this directory. Build:
	PS> Invoke-Build Build

	This command builds the module and installs it to the $ModuleRoot which is
	the working location of the module. The build fails if the module is
	currently in use. Ensure it is not and then repeat.

	The build task Help fails if the help builder Helps is not installed.
	Ignore this or better get and use the script (it is really easy):
	https://github.com/nightroman/Helps
#>

param
(
	$Configuration = 'Release',
	$logfile = $null
)

$project_name = "ShipStationAccess"

# Folder structure:
# \build - Contains all code during the build process
# \build\artifacts - Contains all files during intermidiate bulid process
# \build\output - Contains the final result of the build process
# \release - Contains final release files for upload
# \release\archive - Contains files archived from the previous builds
# \src - Contains all source code
$build_dir = "$BuildRoot\build"
$log_dir = "$BuildRoot\log"
$build_artifacts_dir = "$build_dir\artifacts"
$build_output_dir = "$build_dir\output"
$release_dir = "$BuildRoot\release"
$archive_dir = "$release_dir\archive"

$src_dir = "$BuildRoot\src"
$solution_file = "$src_dir\ShipStationAccess.sln"
	
# Use MSBuild.
use Framework\v4.0.30319 MSBuild

task Clean { 
	exec { MSBuild "$solution_file" /t:Clean /p:Configuration=$configuration /v:quiet } 
	Remove-Item -force -recurse $build_dir -ErrorAction SilentlyContinue | Out-Null
}

task Init Clean, { 
    New-Item $build_dir -itemType directory | Out-Null
    New-Item $build_artifacts_dir -itemType directory | Out-Null
    New-Item $build_output_dir -itemType directory | Out-Null
}

task Build {
	exec { MSBuild "$solution_file" /t:Build /p:Configuration=$configuration /v:minimal /p:OutDir="$build_artifacts_dir\" }
}

task Package  {
	New-Item $build_output_dir\ShipStationAccess\lib\net40 -itemType directory -force | Out-Null
	Copy-Item $build_artifacts_dir\ShipStationAccess.??? $build_output_dir\ShipStationAccess\lib\net40 -PassThru |% { Write-Host "Copied " $_.FullName }
	Copy-Item $build_artifacts_dir\Zayko.Finance.CurrencyConverter.??? $build_output_dir\ShipStationAccess\lib\net40 -PassThru |% { Write-Host "Copied " $_.FullName }
}

# Set $script:Version = assembly version
task Version {
	assert (( Get-Item $build_artifacts_dir\ShipStationAccess.dll ).VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)')
	$script:Version = $matches[1]
}

task Archive {
	New-Item $release_dir -ItemType directory -Force | Out-Null
	New-Item $archive_dir -ItemType directory -Force | Out-Null
	Move-Item -Path $release_dir\*.* -Destination $archive_dir
}

task Zip Version, {
	$release_zip_file = "$release_dir\$project_name.$Version.zip"
	
	Write-Host "Zipping release to: " $release_zip_file
	
	exec { & 7za.exe a $release_zip_file $build_output_dir\ShipStationAccess\lib\net40\* -mx9 }
}

task NuGet Package, Version, {

	Write-Host ================= Preparing ShipStationAccess Nuget package =================
	$text = "ShipStation webservices API wrapper."
	# nuspec
	Set-Content $build_output_dir\ShipStationAccess\ShipStationAccess.nuspec @"
<?xml version="1.0"?>
<package>
	<metadata>
		<id>ShipStationAccess</id>
		<version>$Version-alpha2</version>
		<authors>Slav Ivanyuk</authors>
		<owners>Slav Ivanyuk</owners>
		<projectUrl>https://github.com/slav/ShipStationAccess</projectUrl>
		<licenseUrl>https://raw.github.com/slav/ShipStationAccess/master/License.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<copyright>Copyright (C) Agile Harbor, LLC 2012</copyright>
		<summary>$text</summary>
		<description>$text</description>
		<tags>ShipStation</tags>
		<dependencies> 
			<group targetFramework="net45">
				<dependency id="Netco" version="1.1.0" />
				<dependency id="Microsoft.Data.Edm" version="5.0.0" />
				<dependency id="Microsoft.Data.OData" version="5.0.0" />
				<dependency id="Microsoft.Data.Services.Client" version="5.0.0" />
				<dependency id="System.Spatial" version="5.0.0" />
			</group>
		</dependencies>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack $build_output_dir\ShipStationAccess\ShipStationAccess.nuspec -Output $build_dir }
	
	$pushShipStationAccess = Read-Host 'Push ShipStationAccess ' $Version ' to NuGet? (Y/N)'
	Write-Host $pushShipStationAccess
	if( $pushShipStationAccess -eq "y" -or $pushShipStationAccess -eq "Y" )	{
		Get-ChildItem $build_dir\*.nupkg |% { exec { NuGet push  $_.FullName }}
	}
}

task . Init, Build, Package, Zip, NuGet