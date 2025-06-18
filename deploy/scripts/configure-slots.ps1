# Create staging slot for Store API
az webapp deployment slot create \
  --name app-quartile-store-staging \
  --resource-group rg-quartile-staging \
  --slot staging

# Create staging slot for Function App
az functionapp deployment slot create \
  --name func-quartile-product-staging \
  --resource-group rg-quartile-staging \
  --slot staging