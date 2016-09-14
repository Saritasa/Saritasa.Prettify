# Requires psake to run, see README.md for more details.

# Publish:
# Install AWS SDK for .NET:
# https://aws.amazon.com/sdk-for-net/
#
# Execute this statement before first publish:
# Set-AWSCredentials -AccessKey AKIAJZYMPMLFE3NRX5FA -SecretKey **************************************** -StoreAs Bjs

Framework "4.6"

Import-Module .\scripts\Saritasa.Psake.psd1
Import-Module .\scripts\Saritasa.Build.psd1
Register-HelpTask
Register-UpdateGalleryTask

Properties `
{
    $projectName = 'Saritasa.CodePrettify'
    $urlPrefix = 'http://s3-us-west-2.amazonaws.com/saritasa-code-prettify/'
}

Task 'build-debug' -description '* Build project with debug configuration.' `
{
    Invoke-NugetRestore 'src\Saritasa.Prettify.sln'
    Invoke-SolutionBuild 'src\Saritasa.Prettify.sln' 'debug'
}

Task 'build-release' -description '* Build project with release configuration.' `
{
    Invoke-NugetRestore 'src\Saritasa.Prettify.sln'
    Invoke-SolutionBuild 'src\Saritasa.Prettify.sln' 'release'
}

Task 'publish' -description '* Publish development application using ClickOnce technology.' `
{
    Publish-App 'release'
}

function Publish-App()
{
    param
    (
        [Parameter(Mandatory = $true, HelpMessage = 'Publish target.')]
        [string] $PublishTarget
    )
    
    Import-Module '.\scripts\Saritasa.Publish.psd1'
    Import-Module "${env:ProgramFiles(x86)}\AWS Tools\PowerShell\AWSPowerShell\AWSPowerShell.psd1"

    Set-AWSCredentials -ProfileName Bjs

    $tempDir = "$env:TEMP\" + [System.Guid]::NewGuid().ToString()
    $publishUrl = "$urlPrefix$PublishTarget/"
    Invoke-FullPublish '.\src\Saritasa.Prettify.UI\Saritasa.Prettify.UI.csproj' $tempDir $publishUrl
    Write-S3Object -BucketName 'saritasa-code-prettify' -Folder $tempDir -KeyPrefix $PublishTarget -Recurse -CannedACLName 'public-read'
    Remove-Item $tempDir -Recurse -ErrorAction Stop
    
    Write-Information "Install URL: $publishUrl"
}
