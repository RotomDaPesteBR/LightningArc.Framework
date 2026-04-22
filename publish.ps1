# Script Settings
$ErrorActionPreference = "Stop" # Stops the script if an unhandled error occurs
$config = "Release"

# --- 1. FUNCTION TO GET VERSION ---
function Get-PackageVersionFromProps {
    param(
        [string]$PropsFile = "Directory.Build.props"
    )

    if (-not (Test-Path -Path $PropsFile -PathType Leaf)) {
        Write-Host "WARNING: Properties file '$PropsFile' not found. Using default version '1.0.0'." -ForegroundColor Yellow
        return "1.0.0"
    }

    try {
        # Load the XML file
        [xml]$xmlContent = Get-Content $PropsFile -Raw
        
        # Look for <PackageVersion> directly inside a <PropertyGroup>
        $versionNode = $xmlContent.Project.PropertyGroup.PackageVersion | Select-Object -First 1

        if ($null -eq $versionNode) {
            Write-Host "WARNING: The <PackageVersion> property was not found in '$PropsFile'. Using default version '1.0.0'." -ForegroundColor Yellow
            return "1.0.0"
        }
        
        $version = ""
        if ($versionNode -is [System.Xml.XmlElement]) {
            $version = $versionNode.InnerText
        } else {
            $version = [string]$versionNode
        }

        if ([string]::IsNullOrWhiteSpace($version)) {
            Write-Host "WARNING: The <PackageVersion> property is empty in '$PropsFile'. Using default version '1.0.0'." -ForegroundColor Yellow
            return "1.0.0"
        }
        
        return $version.Trim()
    } catch {
        Write-Host "ERROR analyzing file '$PropsFile'. Using default version '1.0.0'. Error: $($_.Exception.Message)" -ForegroundColor Red
        return "1.0.0"
    }
}

# --- 2. DEFINE VARIABLES WITH THE VERSION ---
$current_version = Get-PackageVersionFromProps -PropsFile "Directory.Build.props" 
$base_output_dir = "publish\packages"
$output_dir = Join-Path -Path $base_output_dir -ChildPath $current_version 

Write-Host "Package Version: $current_version" -ForegroundColor Green
Write-Host "Output Directory: $output_dir" -ForegroundColor Green

# List of projects you want to pack (adjust these paths according to your libraries)
$projects_to_pack = @(
    # Core
    "src\Core\Primitives\LightningArc.Primitives.csproj",
    "src\Core\Primitives.Results\LightningArc.Primitives.Results.csproj",
    "src\Core\Results\LightningArc.Results.csproj",
    "src\Core\Json\LightningArc.Json.csproj",
    "src\Core\Framework\LightningArc.Framework.csproj",

    # Data
    "src\Data\Abstractions\LightningArc.Data.Abstractions.csproj",
    "src\Data\Mappers\Mappers.AutoMapper\LightningArc.Mappers.AutoMapper.csproj",
    "src\Data\Mappers\Mappers.Mapster\LightningArc.Mappers.Mapster.csproj",
    "src\Data\ADO\ADO\LightningArc.Data.ADO.csproj",
    "src\Data\ADO\ADO.SqlBuilder\LightningArc.Data.ADO.SqlBuilder.csproj",
    "src\Data\ADO\ADO.Oracle\LightningArc.Data.ADO.Oracle.csproj",
    "src\Data\ADO\ADO.SqlServer\LightningArc.Data.ADO.SqlServer.csproj",
    "src\Data\EF\EntityFramework\LightningArc.Data.EntityFramework.csproj",

    # AspNetCore
    "src\Web\Results.AspNetCore\LightningArc.Results.AspNetCore.csproj",
    "src\Web\CORS.AspNetCore\LightningArc.CORS.AspNetCore.csproj",
    "src\Web\OpenAPI.AspNetCore\LightningArc.OpenAPI.AspNetCore.csproj",
    "src\Web\AspNetCore\LightningArc.AspNetCore.csproj",

    # Meta
    "src\Meta\Metalama\LightningArc.Metalama.csproj",
    "src\Meta\Metalama.Results\LightningArc.Metalama.Results.csproj"
)

# --- General Clean and Build ---
Write-Host "Cleaning up build artifacts..." -ForegroundColor Cyan
dotnet clean --configuration $config

Write-Host "`nBuilding all projects in the solution..." -ForegroundColor Yellow
dotnet build --configuration $config /p:ExcludeRestorePackageImports=false /p:IsTestProject=false -m:16

# Check the Build result
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nERROR: Build failed (Exit Code: $LASTEXITCODE). Aborting packing." -ForegroundColor Red
    exit 1 
}

# --- Clean Existing Packages (Version Specific) ---
# Removes all existing .nupkg files in the new output directory ($output_dir).
Write-Host "`nBuild completed successfully.`nDeleting .nupkg packages in '$output_dir'..." -ForegroundColor Cyan
if (Test-Path -Path $output_dir -PathType Container) {
    $files_to_delete = Get-ChildItem -Path $output_dir -Filter *.nupkg -File -ErrorAction SilentlyContinue
    if ($files_to_delete.Count -gt 0) {
        Write-Host "Deleting $($files_to_delete.Count) existing .nupkg packages..." -ForegroundColor Cyan
        $files_to_delete | Remove-Item -Force
        Write-Host "Package cleanup complete." -ForegroundColor Cyan
    } else {
        Write-Host "No existing .nupkg packages found for deletion." -ForegroundColor Cyan
    }
} else {
    Write-Host "Output directory '$output_dir' does not exist. It will be created during 'pack'." -ForegroundColor Cyan
}

# --- Packing (Pack) ---
Write-Host "`nStarting project packing..." -ForegroundColor Yellow

$pack_failure = $false

foreach ($project in $projects_to_pack) { 
    Write-Host "`nPacking project: $project" -ForegroundColor DarkYellow
    # The --output directs to publish\packages\($current_version)
    dotnet pack $project --configuration $config --output $output_dir --no-build
    
    # Check the Pack result for each project
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Packing for '$project' failed (Exit Code: $LASTEXITCODE)." -ForegroundColor Red
        $pack_failure = $true
    }
}

# Check the final Pack result
if ($pack_failure) {
    Write-Host "`nERROR: One or more projects failed during packing." -ForegroundColor Red
} else {
    Write-Host "`nPackages generated successfully in '$output_dir'" -ForegroundColor Green
}

Read-Host "Press Enter to continue..."