# Create production resource group
az group create --name rg-quartile-prod --location westeurope

# Deploy resources using Bicep template with production parameters
az deployment group create \
  --resource-group rg-quartile-prod \
  --template-file deploy/main.bicep \
  --parameters deploy/parameters.production.json \
  --parameters sqlAdministratorLoginPassword=<secure-password>