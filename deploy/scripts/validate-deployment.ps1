param(
    [Parameter(Mandatory=$true)]
    [string]$Environment
)

# Test Store API health endpoint
$storeApiUrl = "https://app-quartile-store-$Environment.azurewebsites.net/health"
$storeApiResult = Invoke-WebRequest -Uri $storeApiUrl
if ($storeApiResult.StatusCode -ne 200) {
    Write-Error "Store API health check failed"
    exit 1
}

# Test Function App health endpoint
$functionUrl = "https://func-quartile-product-$Environment.azurewebsites.net/api/health"
$functionResult = Invoke-WebRequest -Uri $functionUrl
if ($functionResult.StatusCode -ne 200) {
    Write-Error "Function App health check failed"
    exit 1
}

# Test database connection
$sqlServer = "sql-quartile-$Environment.database.windows.net"
$database = "QuartileChallengeDb"
$query = "SELECT 1"
try {
    Invoke-Sqlcmd -ServerInstance $sqlServer -Database $database -Query $query
}
catch {
    Write-Error "Database connection test failed"
    exit 1
}

Write-Host "All validation checks passed successfully"