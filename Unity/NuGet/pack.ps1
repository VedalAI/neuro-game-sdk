param (
    [string]$Version
)

$PackageJsonPath = "../Assets/package.json"

if (-Not $Version) {
    if (-Not (Test-Path $PackageJsonPath)) {
        Write-Error "package.json not found at path: $PackageJsonPath"
        exit 1
    }

    $PackageJsonContent = Get-Content -Path $PackageJsonPath -Raw | ConvertFrom-Json
    $Version = $PackageJsonContent.version

    if (-Not $Version) {
        Write-Error "Version not found in package.json"
        exit 1
    }
}

$PackNuspecProps = @{
    "version" = $Version;
    "branch" = $(git rev-parse --abbrev-ref HEAD);
    "commit" = $(git rev-parse HEAD);
}
$PropsString = $PackNuspecProps.GetEnumerator() | ForEach-Object {"$($_.Key)=$($_.Value)"} | Join-String -Separator ';'
Write-Debug $PropsString
dotnet pack NeuroSdk.csproj -p:NuspecProperties=`"$PropsString`"
