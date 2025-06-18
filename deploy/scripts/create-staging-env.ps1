# Create resource group
az group create --name rg-quartile-staging --location westeurope

# Deploy resources using Bicep template
az deployment group create \
  --resource-group rg-quartile-staging \
  --template-file deploy/main.bicep \
  --parameters deploy/parameters.staging.json \
  --parameters sqlAdministratorLoginPassword=<secure-password>