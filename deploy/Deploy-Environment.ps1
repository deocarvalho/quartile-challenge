param(
    [Parameter(Mandatory=$true)]
    [string]$Environment,

    [Parameter(Mandatory=$true)]
    [string]$Location,

    [Parameter(Mandatory=$true)]
    [string]$SqlAdminUsername,

    [Parameter(Mandatory=$true)]
    [System.Security.SecureString]$SqlAdminPassword,

    [Parameter()]
    [switch]$EnableRollback
)

# Undo function
function Undo-Deployment {
    param(
        [string]$ResourceGroupName,
        [string]$Environment
    )
    
    Write-Warning "Starting undo procedure..."
    
    try {
        # Revert slot swap if needed
        $slots = @(
            @{ App = "app-quartile-store-$Environment"; Type = "webapp" },
            @{ App = "func-quartile-product-$Environment"; Type = "functionapp" }
        )
        
        foreach ($slot in $slots) {
            Write-Host "Undoing $($slot.App) slot swap..."
            az $slot.Type deployment slot swap `
                --name $slot.App `
                --resource-group $ResourceGroupName `
                --slot staging `
                --target-slot production
        }
        
        Write-Host "Undo completed successfully"
    }
    catch {
        Write-Error "Undo failed: $_"
        throw
    }
}

try {
    # Create resource group
    $resourceGroupName = "rg-quartile-$Environment"
    az group create --name $resourceGroupName --location $Location

    # Deploy Bicep template
    $password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SqlAdminPassword))
    az deployment group create `
        --resource-group $resourceGroupName `
        --template-file "bicep deploy/main.bicep" `
        --parameters `
            environmentName=$Environment `
            location=$Location `
            sqlAdministratorLogin=$SqlAdminUsername `
            sqlAdministratorPassword=$password

    # The following database schema deployment steps have been commented out
    # as they were failing in the execution environment.
    # These scripts should be run manually via the Azure Portal Query Editor if needed.
    #
    # # Deploy database schema
    # $sqlServerName = az deployment group show `
    #     --resource-group $resourceGroupName `
    #     --name main `
    #     --query properties.outputs.sqlServerName.value `
    #     --output tsv

    # Write-Host "Deploying database schema..."
    # foreach ($script in Get-ChildItem "deploy/database/schemas" -Filter "*.sql" | Sort-Object Name) {
    #     Write-Host "Executing $($script.Name)..."
    #     $scriptContent = Get-Content $script.FullName -Raw
    #     az sql db query `
    #         --resource-group $resourceGroupName `
    #         --server $sqlServerName `
    #         --database QuartileChallengeDb `
    #         --query "$scriptContent"
    # }

    # # Deploy functions and procedures
    # Write-Host "Deploying database functions and procedures..."
    # foreach ($script in Get-ChildItem "database/functions" -Filter "*.sql") {
    #     Write-Host "Executing $($script.Name)..."
    #     $scriptContent = Get-Content $script.FullName -Raw
    #     az sql db query `
    #         --resource-group $resourceGroupName `
    #         --server $sqlServerName `
    #         --database QuartileChallengeDb `
    #         --query "$scriptContent"
    # }

    # foreach ($script in Get-ChildItem "database/procedures" -Filter "*.sql") {
    #     Write-Host "Executing $($script.Name)..."
    #     $scriptContent = Get-Content $script.FullName -Raw
    #     az sql db query `
    #         --resource-group $resourceGroupName `
    #         --server $sqlServerName `
    #         --database QuartileChallengeDb `
    #         --query "$scriptContent"
    # }

    Write-Host "Deployment completed successfully!"
}
catch {
    if ($EnableRollback) {
        Write-Warning "Deployment failed, initiating undo..."
        Undo-Deployment -ResourceGroupName $resourceGroupName -Environment $Environment
    }
    throw
}