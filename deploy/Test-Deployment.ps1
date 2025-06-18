param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('staging','production')]
    [string]$Environment,
    
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName
)

# Import Azure credentials from pipeline variables
$ErrorActionPreference = "Stop"

Write-Host "Starting deployment validation for $Environment environment..."

# Test Resource Group
Write-Host "Checking Resource Group..."
$rg = az group show --name $ResourceGroupName | ConvertFrom-Json
if (-not $rg) {
    throw "Resource Group $ResourceGroupName not found"
}

# Test Key Vault
Write-Host "Checking Key Vault..."
$kvName = "kv-quartile-$Environment"
$kv = az keyvault show --name $kvName --resource-group $ResourceGroupName | ConvertFrom-Json
if (-not $kv) {
    throw "Key Vault $kvName not found"
}

# Test Store API
$storeApiUrl = "https://app-quartile-store-$Environment.azurewebsites.net"
Write-Host "Testing Store API at $storeApiUrl..."

try {
    # Health Check
    $response = Invoke-WebRequest -Uri "$storeApiUrl/health" -Method Get
    if ($response.StatusCode -ne 200) {
        throw "Store API health check failed with status code $($response.StatusCode)"
    }
    
    # Test Store API Endpoints
    $endpoints = @(
        @{ Method = "GET"; Path = "/api/stores"; ExpectedStatus = 200 },
        @{ Method = "GET"; Path = "/api/stores/company/$($TestCompanyId)"; ExpectedStatus = 200 }
    )
    
    foreach ($endpoint in $endpoints) {
        $response = Invoke-WebRequest -Uri "$storeApiUrl$($endpoint.Path)" -Method $endpoint.Method
        if ($response.StatusCode -ne $endpoint.ExpectedStatus) {
            throw "Store API endpoint $($endpoint.Path) failed with status code $($response.StatusCode)"
        }
    }
    
    Write-Host "Store API validation passed!"
}
catch {
    Write-Error "Store API validation failed: $_"
    exit 1
}

# Test Function App
$functionUrl = "https://func-quartile-product-$Environment.azurewebsites.net"
Write-Host "Testing Function App at $functionUrl..."

try {
    # Health Check
    $response = Invoke-WebRequest -Uri "$functionUrl/api/health" -Method Get
    if ($response.StatusCode -ne 200) {
        throw "Function App health check failed with status code $($response.StatusCode)"
    }
    
    # Test Function Endpoints
    $endpoints = @(
        @{ Method = "GET"; Path = "/api/products"; ExpectedStatus = 200 },
        @{ Method = "GET"; Path = "/api/products/store/$($TestStoreId)"; ExpectedStatus = 200 }
    )
    
    foreach ($endpoint in $endpoints) {
        $response = Invoke-WebRequest -Uri "$functionUrl$($endpoint.Path)" -Method $endpoint.Method
        if ($response.StatusCode -ne $endpoint.ExpectedStatus) {
            throw "Function App endpoint $($endpoint.Path) failed with status code $($response.StatusCode)"
        }
    }
    
    Write-Host "Function App validation passed!"
}
catch {
    Write-Error "Function App validation failed: $_"
    exit 1
}

# Test SQL Database
Write-Host "Testing SQL Database connectivity..."
$sqlServerName = "sql-quartile-$Environment"
try {
    $testQuery = "SELECT 1 AS TestColumn"
    $result = az sql db query `
        --resource-group $ResourceGroupName `
        --server $sqlServerName `
        --database "QuartileChallengeDb" `
        --query "$testQuery" | ConvertFrom-Json
        
    if (-not $result) {
        throw "SQL Database connectivity test failed"
    }
    Write-Host "SQL Database validation passed!"
}
catch {
    Write-Error "SQL Database validation failed: $_"
    exit 1
}

# Test Deployment Slots
Write-Host "Checking deployment slots..."
$slots = @(
    @{ App = "app-quartile-store-$Environment"; Type = "webapp" },
    @{ App = "func-quartile-product-$Environment"; Type = "functionapp" }
)

foreach ($slot in $slots) {
    $slotInfo = az $slot.Type deployment slot list `
        --name $slot.App `
        --resource-group $ResourceGroupName | ConvertFrom-Json
        
    if (-not ($slotInfo | Where-Object { $_.name -eq 'staging' })) {
        throw "$($slot.App) staging slot not found"
    }
}

Write-Host "Deployment slot validation passed!"

# Test SSL/TLS Configuration
Write-Host "Checking SSL/TLS configuration..."
foreach ($app in @($storeApiUrl, $functionUrl)) {
    $sslTest = Invoke-WebRequest -Uri $app -Method Get
    if (-not $sslTest.StatusCode -eq 200) {
        throw "SSL/TLS validation failed for $app"
    }
}

Write-Host "SSL/TLS validation passed!"

Write-Host "All deployment validations completed successfully!"