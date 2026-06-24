$dotnet = "$env:USERPROFILE\.dotnet\dotnet.exe"
$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet"
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
Set-Location $PSScriptRoot
Write-Host "Restoring packages..."
& $dotnet restore
Write-Host "Starting API at http://localhost:5127/swagger ..."
& $dotnet run --project GroceryEcommerceApi.csproj --urls "http://localhost:5127"
