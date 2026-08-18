[CmdletBinding()]
param(
    [string]$Branch = "main",
    [string]$BaseOutput = "./docs/api",
    [string]$ArtifactsPath = "./.artifacts/bin"
)

$ErrorActionPreference = "Stop"

$fileRepoUrl = "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/$Branch"
$rootPath = (Get-Item .).FullName
$tempSourceMarker = "https://TEMP_SOURCE_LINK"

# Clean previous output
Remove-Item -Path "$BaseOutput/*" -Recurse -Force -ErrorAction SilentlyContinue

# Public API projects to document
$docProjects = @(
    "LightningArc.Primitives",
    "LightningArc.Primitives.Results",
    "LightningArc.Json",
    "LightningArc.Results",
    "LightningArc.Validations",
    "LightningArc.Validations.DependencyInjection",
    "LightningArc.Data.Abstractions",
    "LightningArc.Data.ADO",
    "LightningArc.Data.ADO.SqlBuilder",
    "LightningArc.Data.ADO.SqlServer",
    "LightningArc.Data.ADO.Oracle",
    "LightningArc.Data.EntityFramework",
    "LightningArc.Mappers.AutoMapper",
    "LightningArc.Mappers.Mapster",
    "LightningArc.Metalama.Results",
    "LightningArc.Metalama",
    "LightningArc.CORS.AspNetCore",
    "LightningArc.OpenAPI.AspNetCore",
    "LightningArc.Results.AspNetCore"
)

# Find eligible DLLs
$allDlls = Get-ChildItem -Recurse -Filter "*.dll" -Path $ArtifactsPath | Where-Object {
    $docProjects -contains $_.BaseName -and
    (Test-Path ($_.FullName -replace '\.dll$', '.xml'))
}

# Process and generate docs
$groupedProjects = $allDlls | Group-Object Name

foreach ($group in $groupedProjects) {
    $availableTfms = $group.Group | ForEach-Object {
        if ($_.FullName -match '(debug|release|dev)_(net\d+\.?\d*|netstandard\d+\.?\d*)') {
            $matches[2]
        }
    } | Sort-Object -Unique -Descending

    $selectedDll = $null
    foreach ($tfm in $availableTfms) {
        $selectedDll = $group.Group | Where-Object { $_.FullName -match "(debug|release|dev)_$([regex]::Escape($tfm))" } | Select-Object -First 1
        if ($selectedDll) { break }
    }

    if ($selectedDll) {
        $projectName = $selectedDll.BaseName
        $projectOutput = "$BaseOutput/$projectName"

        $typeToFileMap = @{}
        $projFile = Get-ChildItem -Path "./src" -Filter "$projectName.csproj" -Recurse | Select-Object -First 1

        if ($projFile) {
            $csFiles = Get-ChildItem -Path $projFile.DirectoryName -Filter "*.cs" -Recurse | 
                       Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' }

            foreach ($csFile in $csFiles) {
                $relPath = $csFile.FullName.Substring($rootPath.Length).Trim('\', '/').Replace('\', '/')
                $typeToFileMap[$csFile.Name] = $relPath

                $code = Get-Content $csFile.FullName -Raw
                $typeMatches = [regex]::Matches($code, '\b(class|struct|interface|record|enum)\s+([A-Za-z0-9_]+)\b')
                foreach ($m in $typeMatches) {
                    $virtualFile = "$($m.Groups[2].Value).cs"
                    if (-not $typeToFileMap.ContainsKey($virtualFile)) {
                        $typeToFileMap[$virtualFile] = $relPath
                    }
                }
            }
        }

        dotnet run --project tools/Documentation.Generator/LightningArc.Documentation.Generator.csproj -- $selectedDll.FullName $projectOutput --source "$tempSourceMarker" --newline lf

        $entryFile = "$projectOutput/$projectName.md"
        if (Test-Path $entryFile) {
            $lines = Get-Content $entryFile
            $rawContent = $lines -join "`n"

            if ($lines.Count -le 3 -or $rawContent.Trim() -match "^#.*assembly`r?`n`r?`n<!-- DO NOT EDIT") {
                Remove-Item -Path $projectOutput -Recurse -Force -ErrorAction SilentlyContinue
            } else {
                Move-Item $entryFile "$projectOutput/README.md" -Force

                $escapedTarget = [regex]::Escape("$projectName.md")
                $readmePattern = '(\[[^\]]+\]\()((?:\.\./|\./)*)' + $escapedTarget + '(\))'
                $tempSourceRegex = '\[([^\]]+\.cs)\]\(' + [regex]::Escape($tempSourceMarker) + '[^\)]*\)'

                Get-ChildItem -Path $projectOutput -Recurse -Filter "*.md" | ForEach-Object {
                    $mdFile = $_
                    $mdContent = Get-Content $mdFile.FullName -Raw

                    $updatedContent = $mdContent -replace $readmePattern, '${1}${2}README.md${3}'

                    $updatedContent = [regex]::Replace($updatedContent, $tempSourceRegex, {
                        param($match)
                        $fileName = $match.Groups[1].Value
                        $targetRelPath = $null

                        if ($typeToFileMap.ContainsKey($fileName)) {
                            $targetRelPath = $typeToFileMap[$fileName]
                        } else {
                            $fallbackFile = Get-ChildItem -Path "./src" -Filter $fileName -Recurse | Select-Object -First 1
                            if ($fallbackFile) {
                                $targetRelPath = $fallbackFile.FullName.Substring($rootPath.Length).Trim('\', '/').Replace('\', '/')
                            } else {
                                $typeName = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
                                $contentMatch = Get-ChildItem -Path "./src" -Filter "*.cs" -Recurse |
                                                Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' } |
                                                Select-String -Pattern "\b(class|struct|interface|record|enum)\s+$typeName\b" -List |
                                                Select-Object -First 1
                                if ($contentMatch) {
                                    $targetRelPath = $contentMatch.Path.Substring($rootPath.Length).Trim('\', '/').Replace('\', '/')
                                }
                            }
                        }

                        if ($targetRelPath) {
                            return "[$fileName]($fileRepoUrl/$targetRelPath)"
                        } else {
                            Write-Warning "[$projectName] Could not resolve source link for $fileName"
                            return "[$fileName]($fileRepoUrl/src)"
                        }
                    })

                    Set-Content -Path $mdFile.FullName -Value $updatedContent -NoNewline
                }
            }
        }
    }
}