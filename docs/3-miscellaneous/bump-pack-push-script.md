# Bump versions, build, pack and push script

This is a development script that makes it easier to test incomplete versions of FatNetLib.
It requires a demo solution with three projects:

* Server
* Client
* Common

This script does the following things:

* Increases the versions of FatNetLib, FatNetLib.Json and FatNetLib.MicrosoftLogging projects
* Packs Nuget packages of these projects
* Pushes these Nuget packages to the local repository
* Increases FatNetLib version for Server, Client and Common projects in a demo solution

For use this script, you need to replace the following placeholders with absolute paths:

* {PATH_TO_FAT_NET_LIB_HERE} — path to FatNetLib solution
* {PATH_TO_FAT_NET_LIB_DEMO_HERE} — path to FatNetLib Demo solution
* {PATH_TO_YOUR_LOCAL_REPO_HERE} — path where the Nuget packages will be pushed

## Powershell

```powershell
$fatNetLibPath = '{PATH_TO_FAT_NET_LIB_HERE}';
$fatNetLibDemoPath = '{PATH_TO_FAT_NET_LIB_DEMO_HERE}';
$localRepoPath = '{PATH_TO_YOUR_LOCAL_REPO_HERE}';

$csprojName = "$fatNetLibPath/FatNetLib.Core/FatNetLib.Core.csproj"
$xml = [xml](Get-Content $csprojName);
$version = [version]$xml.Project.PropertyGroup.Version;
if (-1 -eq $version.Revision)
{
    $version = [version]::New($version.Major, $version.Minor, $version.Build, 1);
}
else
{
    $version = [version]::New($version.Major, $version.Minor, $version.Build, $version.Revision + 1);
}
$xml.Project.PropertyGroup.Version = $version.ToString();
$xml.Save($csprojName);

$csprojName = "$fatNetLibPath/FatNetLib.Json/FatNetLib.Json.csproj"
$xml = [xml](Get-Content $csprojName);
$version = [version]$xml.Project.PropertyGroup.Version;
if (-1 -eq $version.Revision)
{
    $version = [version]::New($version.Major, $version.Minor, $version.Build, 1);
}
else
{
    $version = [version]::New($version.Major, $version.Minor, $version.Build, $version.Revision + 1);
}
$xml.Project.PropertyGroup.Version = $version.ToString();
$xml.Save($csprojName);

$csprojName = "$fatNetLibPath/FatNetLib.MicrosoftLogging/FatNetLib.MicrosoftLogging.csproj"
$xml = [xml](Get-Content $csprojName);
$version = [version]$xml.Project.PropertyGroup.Version;
if (-1 -eq $version.Revision)
{
    $version = [version]::New($version.Major, $version.Minor, $version.Build, 1);
}
else
{
    $version = [version]::New($version.Major, $version.Minor, $version.Build, $version.Revision + 1);
}
$xml.Project.PropertyGroup.Version = $version.ToString();
$xml.Save($csprojName);

rm "$fatNetLibPath/FatNetLib.Core\bin\Release\*.nupkg";
rm "$fatNetLibPath/FatNetLib.Json\bin\Release\*.nupkg";
rm "$fatNetLibPath/FatNetLib.MicrosoftLogging\bin\Release\*.nupkg";

dotnet build --no-incremental --configuration Release;
dotnet pack "$fatNetLibPath/FatNetLib.Core/FatNetLib.Core.csproj" --no-build --configuration Release;
dotnet pack "$fatNetLibPath/FatNetLib.Json/FatNetLib.Json.csproj" --no-build --configuration Release;
dotnet pack "$fatNetLibPath/FatNetLib.MicrosoftLogging/FatNetLib.MicrosoftLogging.csproj" --no-build --configuration Release;
dotnet nuget push "${projectPath}FatNetLib.Core/bin/Release/**.nupkg" --source $localRepoPath;
dotnet nuget push "${projectPath}FatNetLib.Json/bin/Release/**.nupkg" --source $localRepoPath;
dotnet nuget push "${projectPath}FatNetLib.MicrosoftLogging/bin/Release/**.nupkg" --source $localRepoPath;

$csprojName = "$fatNetLibDemoPath/Client/Client.csproj";
$xml = [xml](Get-Content $csprojName);
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.Core" };
$package.SetAttribute("Version",$version.ToString());
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.Json" };
$package.SetAttribute("Version",$version.ToString());
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.MicrosoftLogging" };
$package.SetAttribute("Version",$version.ToString());
$xml.Save($csprojName);


$csprojName = "$fatNetLibDemoPath/Server/Server.csproj";
$xml = [xml](Get-Content $csprojName);
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.Core" };
$package.SetAttribute("Version",$version.ToString());
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.Json" };
$package.SetAttribute("Version",$version.ToString());
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.MicrosoftLogging" };
$package.SetAttribute("Version",$version.ToString());
$xml.Save($csprojName);

$csprojName = "$fatNetLibDemoPath/Common/Common.csproj";
$xml = [xml](Get-Content $csprojName);
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.Core" };
$package.SetAttribute("Version",$version.ToString());
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.Json" };
$package.SetAttribute("Version",$version.ToString());
$package = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "FatNetLib.MicrosoftLogging" };
$package.SetAttribute("Version",$version.ToString());
$xml.Save($csprojName);
```

## Bash

You have to install `xmlstarlet` before using the script

```bash
fatNetLibPath='{PATH_TO_FAT_NET_LIB_HERE}';
fatNetLibDemoPath='{PATH_TO_FAT_NET_LIB_DEMO_HERE}';
localRepoPath='{PATH_TO_YOUR_LOCAL_REPO_HERE}';

function getNewVersion() {
    xpath='/Project/PropertyGroup/Version/text()'
    csprojPath=$1
    version="$(xmlstarlet sel -t -v $xpath $csprojPath)"
    major=$(echo $version | cut -d. -f1)
    minor=$(echo $version | cut -d. -f2)
    patch=$(echo $version | cut -d. -f3)
    build=$(echo $version | cut -d. -f4)
    echo $major.$minor.$patch.$(($build+1))
}

newVersion=$(getNewVersion ${fatNetLibPath}/FatNetLib.Core/FatNetLib.Core.csproj)

function bumpProjectVersion() {
  xpath='/Project/PropertyGroup/Version/text()'
  csprojPath=$1
  version=$2
  xmlstarlet ed --inplace --pf --omit-decl -u $xpath -v $version $csprojPath
}

bumpProjectVersion ${fatNetLibPath}/FatNetLib.Core/FatNetLib.Core.csproj "$newVersion"
bumpProjectVersion ${fatNetLibPath}/FatNetLib.Json/FatNetLib.Json.csproj "$newVersion"
bumpProjectVersion ${fatNetLibPath}/FatNetLib.MicrosoftLogging/FatNetLib.MicrosoftLogging.csproj "$newVersion"

rm ${fatNetLibPath}/FatNetLib.Core/bin/Release/*.nupkg
rm ${fatNetLibPath}/FatNetLib.Json/bin/Release/*.nupkg
rm ${fatNetLibPath}/FatNetLib.MicrosoftLogging/bin/Release/*.nupkg

dotnet build --no-incremental --configuration Release;
dotnet pack "${fatNetLibPath}/FatNetLib.Core/FatNetLib.Core.csproj" --no-build --configuration Release;
dotnet pack "${fatNetLibPath}/FatNetLib.Json/FatNetLib.Json.csproj" --no-build --configuration Release;
dotnet pack "${fatNetLibPath}/FatNetLib.MicrosoftLogging/FatNetLib.MicrosoftLogging.csproj" --no-build --configuration Release;
dotnet nuget push "${fatNetLibPath}/FatNetLib.Core/bin/Release/**.nupkg" --source $localRepoPath;
dotnet nuget push "${fatNetLibPath}/FatNetLib.Json/bin/Release/**.nupkg" --source $localRepoPath;
dotnet nuget push "${fatNetLibPath}/FatNetLib.MicrosoftLogging/bin/Release/**.nupkg" --source $localRepoPath;


function bumpDependencyVersion() {
  csprojPath=$1
  version=$2
  coreXPath="/Project/ItemGroup/PackageReference[@Include='FatNetLib.Core']/@Version"
  jsonXPath="/Project/ItemGroup/PackageReference[@Include='FatNetLib.Json']/@Version"
  msLogXPath="/Project/ItemGroup/PackageReference[@Include='FatNetLib.MicrosoftLogging']/@Version"
  xmlstarlet ed --inplace --pf --omit-decl -u "$coreXPath" -v "$version" "$csprojPath"
  xmlstarlet ed --inplace --pf --omit-decl -u "$jsonXPath" -v "$version" "$csprojPath"
  xmlstarlet ed --inplace --pf --omit-decl -u "$msLogXPath" -v "$version" "$csprojPath"
}

bumpDependencyVersion "$fatNetLibDemoPath/Common/Common.csproj" "$newVersion"
bumpDependencyVersion "$fatNetLibDemoPath/Client/Client.csproj" "$newVersion"
bumpDependencyVersion "$fatNetLibDemoPath/Server/Server.csproj" "$newVersion"

```
