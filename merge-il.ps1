<#
    .SYNOPSIS
    Merging dlls for prettifyer
#>

param(
    [string]$pathToExe = "\src\Saritasa.Prettify.Cli\bin\Debug\Saritasa.Prettify.ConsoleApp.exe",
    [string]$dllsToMergeDirectory = "\src\Saritasa.Prettify.Cli\bin\Debug\",
    [string]$outName = "output.exe"
)


$root = $MyInvocation.MyCommand.Definition
$directory = [string][System.IO.Path]::GetDirectoryName($root)
$directory = $directory.Replace(" ", "` ")
$ilMergeFile = "$directory\tools\ilmerge\ILMerge.exe"

 Write-Host "Current directory: $directory" -ForegroundColor "Green"
 Write-Host "ILMerge.exe location : $ilMergeFile" -ForegroundColor "Green"

 #validation block

 if($pathToExe -eq "")
 {
     throw "Please specify path to exe which will merged with dll's, -pathToExe"
 }

 $pathToExe = $directory + $pathToExe

 if(![System.IO.File]::Exists($pathToExe))
 {
     throw "merging .exe file not exists, searching in $pathToExe"
 }

 if($dllsToMergeDirectory -eq "")
 {
     throw "Plase specify full directory for dlls"
 }

 $dllsToMergeDirectory = $directory + $dllsToMergeDirectory

 if(![System.IO.Directory]::Exists($dllsToMergeDirectory))
 {
     throw "Directory not exists, -dllToMergeDirectory, searching in $dllsToMergeDirectory"
 }

 if([System.IO.File]::Exists($ilMergeFile) -eq $false)
 {
     throw "IlMerge file not found, searched at " + $ilMergeFile
 }

$pathsForDllsToMerge = [System.IO.Directory]::EnumerateFiles($dllsToMergeDirectory, "*.dll")

$command = "/log /out:$outName $pathToExe "

$dllsPathJoined = [System.String]::Join('" "', $pathsForDllsToMerge)

$command += $dllsPathJoined

Write-Host "Command which will executed $command" -ForegroundColor "Blue"

&"$ilMergeFile" $command 



