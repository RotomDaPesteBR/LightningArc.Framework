[CmdletBinding()]
param(
    [string]$Source = "AGENTS.md",
    [string]$Target = "CLAUDE.md"
)

$ErrorActionPreference = "Stop"

if (Test-Path $Source) {
    Copy-Item -Path $Source -Destination $Target -Force
    Write-Host "Successfully copied $Source to $Target"
} else {
    Write-Error "Source file '$Source' not found."
}