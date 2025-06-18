param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('staging', 'production')]
    [string]$SourceSlot,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet('staging', 'production')]
    [string]$TargetSlot
)

# Swap Store API slots
Write-Host "Swapping Store API slots..."
az webapp deployment slot swap \
    --name app-quartile-store-prod \
    --resource-group rg-quartile-prod \
    --slot $SourceSlot \
    --target-slot $TargetSlot

# Swap Function App slots
Write-Host "Swapping Function App slots..."
az functionapp deployment slot swap \
    --name func-quartile-product-prod \
    --resource-group rg-quartile-prod \
    --slot $SourceSlot \
    --target-slot $TargetSlot