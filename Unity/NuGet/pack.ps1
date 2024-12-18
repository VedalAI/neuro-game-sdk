param (
    # you can get version from git if your tag names are semver-compatible
    #[Parameter()]
    #[string]$Version = $(git describe --tags --abbrev=0)
    [Parameter(Mandatory=$true)]
    [string]$Version
)
$PackNuspecProps = @{
    "version" = $Version;
    "branch" = $(git rev-parse --abbrev-ref HEAD);
    "commit" = $(git rev-parse HEAD);
}
$PropsString = $PackNuspecProps.GetEnumerator() | ForEach-Object {"$($_.Key)=$($_.Value)"} | Join-String -Separator ';'
Write-Debug $PropsString
dotnet pack NeuroSdk.csproj -p:NuspecProperties=`"$PropsString`"
