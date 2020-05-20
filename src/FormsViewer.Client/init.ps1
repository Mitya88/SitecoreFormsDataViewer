$applicationName = "SPEAK3Application";
$applicationTitle = "Hello SPEAK3!";
$instanceWebRoot = "C:\\inetpub\\wwwroot\\scMityaTest93.local";
$instanceHost = "https://scmityatest93.local"

Write-Host "Updating Speak3 Start template"

$packageJson = Join-Path $PSScriptRoot "package.json"
$indexHtml = Join-Path $PSScriptRoot "src\index.html"
$appComponent = Join-Path $PSScriptRoot "src\app\app.component.html"

$packageJsonContent = Get-Content $packageJson
$packageJsonContent.Replace("{applicationName}",$applicationName) | Set-Content $packageJson
$packageJsonContent = Get-Content $packageJson
$packageJsonContent.Replace("{instanceWebRoot}",$instanceWebRoot) | Set-Content $packageJson
$packageJsonContent = Get-Content $packageJson
$packageJsonContent.Replace("{instanceHost}",$instanceHost) | Set-Content $packageJson

$indexHtmlContent = Get-Content $indexHtml
$indexHtmlContent.Replace("{applicationTitle}", $applicationTitle) | Set-Content $indexHtml

$appComponentContent = Get-Content $appComponent
$appComponentContent.Replace("{applicationTitle}", $applicationTitle) | Set-Content $appComponent