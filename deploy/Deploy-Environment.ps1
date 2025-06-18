param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('dev','staging','prod')]
    [string]$Environment,

    [Parameter(Mandatory=$true)]
    [string]$Location,

    [Parameter(Mandatory=$true)]
    [string]$SqlAdminUsername,

    [Parameter(Mandatory=$true)]
    [string]$SqlAdminPassword,

    [Parameter()]
    [switch]$EnableRollback
)

# Rollback function
function Rollback-Deployment {
    param(
        [string]$ResourceGroupName,
        [string]$Environment
    )
    
    Write-Warning "Starting rollback procedure..."
    
    try {
        # Revert slot swap if needed
        $slots = @(
            @{ App = "app-quartile-store-$Environment"; Type = "webapp" },
            @{ App = "func-quartile-product-$Environment"; Type = "functionapp" }
        )
        
        foreach ($slot in $slots) {
            Write-Host "Rolling back $($slot.App) slot swap..."
            az $slot.Type deployment slot swap `
                --name $slot.App `
                --resource-group $ResourceGroupName `
                --slot staging `
                --target-slot production
        }
        
        Write-Host "Rollback completed successfully"
    }
    catch {
        Write-Error "Rollback failed: $_"
        throw
    }
}

try {
    # Create resource group
    $resourceGroupName = "rg-quartile-$Environment"
    az group create --name $resourceGroupName --location $Location

    # Deploy Bicep template
    az deployment group create `
        --resource-group $resourceGroupName `
        --template-file deploy/main.bicep `
        --parameters `
            environmentName=$Environment `
            location=$Location `
            sqlAdministratorLogin=$SqlAdminUsername `
            sqlAdministratorPassword=$SqlAdminPassword

    # Deploy database schema
    $sqlServerName = az deployment group show `
        --resource-group $resourceGroupName `
        --name main `
        --query properties.outputs.sqlServerName.value `
        --output tsv

    Write-Host "Deploying database schema..."
    foreach ($script in Get-ChildItem "deploy/database/schemas" -Filter "*.sql" | Sort-Object Name) {
        Write-Host "Executing $($script.Name)..."
        az sql db execute `
            --resource-group $resourceGroupName `
            --server $sqlServerName `
            --name QuartileChallengeDb `
            --file $script.FullName
    }

    # Deploy functions and procedures
    Write-Host "Deploying database functions and procedures..."
    foreach ($script in Get-ChildItem "deploy/database/functions" -Filter "*.sql") {
        Write-Host "Executing $($script.Name)..."
        az sql db execute `
            --resource-group $resourceGroupName `
            --server $sqlServerName `
            --name QuartileChallengeDb `
            --file $script.FullName
    }

    foreach ($script in Get-ChildItem "deploy/database/procedures" -Filter "*.sql") {
        Write-Host "Executing $($script.Name)..."
        az sql db execute `
            --resource-group $resourceGroupName `
            --server $sqlServerName `
            --name QuartileChallengeDb `
            --file $script.FullName
    }

    Write-Host "Deployment completed successfully!"
}
catch {
    if ($EnableRollback) {
        Write-Warning "Deployment failed, initiating rollback..."
        Rollback-Deployment -ResourceGroupName $resourceGroupName -Environment $Environment
    }
    throw
}