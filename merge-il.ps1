<#
    .SYNOPSIS
    Merging dlls for prettifyer
#>

param(
    [string]$pathToExe = "\src\Saritasa.Prettify.Cli\bin\Debug\Saritasa.Prettify.ConsoleApp.exe",
    [string]$dllsToMergeDirectory = "\src\Saritasa.Prettify.Cli\bin\Debug\",
    [string]$outName = "Saritasa.Prettify.ConsoleApp.Merged.exe"
)

$mergingFileName = [System.IO.Path]::GetFileName($pathToExe)

$root = $MyInvocation.MyCommand.Definition
$directory = [string][System.IO.Path]::GetDirectoryName($root)
$directory = $directory.Replace(" ", "` ")
$ilMergeFile = "$directory\tools\ilmerge\ILMerge.exe"
$ilMergeName = [System.IO.Path]::GetFileName($ilMergeFile)

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

$tempPath = Join-Path $PSScriptRoot "Temp"

[System.IO.Directory]::CreateDirectory($tempPath)

$dllsPathJoined = ''

foreach($dll in $pathsForDllsToMerge)
{
    $dllName = [System.IO.Path]::GetFileName($dll)
    $tempDllPath = $tempPath + "\" + $dllName
    [System.IO.File]::Copy($dll, $tempDllPath, $true)

    $dllsPathJoined += ' "' + $tempDllPath + '" '
}

$ilMergeTempPath = Join-Path $tempPath $ilMergeName 

[System.IO.File]::Copy($ilMergeFile, $ilMergeTempPath, $true)

$pathToMergingExe = Join-Path $tempPath $mergingFileName

[System.IO.File]::Copy($pathToExe, $pathToMergingExe, $true)

$fullPathOfMergingExe = [System.IO.Path]::GetFullPath($pathToExe)

$dllsPathJoined = '"' + $fullPathOfMergingExe + '"' + $dllsPathJoined

$tempOut = Join-Path $tempPath $outName

$command = '/ndebug /copyattrs /targetplatform:4.0,"C:\Windows\Microsoft.NET\Framework64\v4.0.30319" /out:"' + $tempOut + '" '


$command += $dllsPathJoined

$command = ' ' + $command

Write-Host "Command which will executed $command" -ForegroundColor "Blue"

Set-Location $tempPath

Invoke-Expression "& `.\ILMerge.exe $command"

$pathToCopy = Join-Path $PSScriptRoot $mergingFileName

Copy-Item $tempOut $pathToCopy






