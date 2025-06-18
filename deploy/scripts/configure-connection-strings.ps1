# Store API connection string
az webapp config connection-string set \
  --resource-group rg-quartile-staging \
  --name app-quartile-store-staging \
  --settings DefaultConnection="Server=sql-quartile-staging.database.windows.net;Database=QuartileChallengeDb;User Id=quartileadmin;Password=<secure-password>" \
  --connection-string-type SQLAzure

# Function App connection string
az functionapp config connection-string set \
  --resource-group rg-quartile-staging \
  --name func-quartile-product-staging \
  --settings DefaultConnection="Server=sql-quartile-staging.database.windows.net;Database=QuartileChallengeDb;User Id=quartileadmin;Password=<secure-password>" \
  --connection-string-type SQLAzure