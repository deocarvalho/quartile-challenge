param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,
    
    [Parameter(Mandatory=$true)]
    [string]$Username,
    
    [Parameter(Mandatory=$true)]
    [string]$Password
)

# Function to execute SQL scripts
function Execute-SqlScript {
    param(
        [string]$scriptPath
    )
    Write-Host "Executing $scriptPath..."
    Invoke-Sqlcmd -ServerInstance $ServerName `
                 -Database $DatabaseName `
                 -Username $Username `
                 -Password $Password `
                 -InputFile $scriptPath `
                 -QueryTimeout 120
}

# Create schemas and tables
Write-Host "Creating database schemas..."
Get-ChildItem "schemas" -Filter "*.sql" | Sort-Object Name | ForEach-Object {
    Execute-SqlScript $_.FullName
}

# Create functions
Write-Host "Creating functions..."
Get-ChildItem "functions" -Filter "*.sql" | ForEach-Object {
    Execute-SqlScript $_.FullName
}

# Create stored procedures
Write-Host "Creating stored procedures..."
Get-ChildItem "procedures" -Filter "*.sql" | ForEach-Object {
    Execute-SqlScript $_.FullName
}

Write-Host "Database deployment completed successfully."