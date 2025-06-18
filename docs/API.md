# Quartile Challenge API Documentation

## Overview
This API provides endpoints for managing stores and products in a multi-company environment.

### Base URLs
- Local: `https://localhost:5001`
- Staging: `https://app-quartile-store-staging.azurewebsites.net`
- Production: `https://app-quartile-store-prod.azurewebsites.net`

## Authentication
All API endpoints require authentication using JWT Bearer tokens.

## Store API

### Get All Stores
```
GET /api/stores
```
Returns all active stores accessible to the authenticated user.

#### Response
```json
[
  {
    "id": "guid", 
    "companyId": "guid", 
    "name": "string", 
    "location": "string", 
    "createdAt": "datetime", 
    "modifiedAt": "datetime", 
    "isActive": true 
  } 
]
```
### Get Store by ID
`GET /api/stores/{id}`
Returns a specific store by its ID.

#### Parameters
- `id` (guid, required): The store's unique identifier

#### Response
```json
{ 
  "id": "guid", 
  "companyId": "guid", 
  "name": "string", 
  "location": "string", 
  "createdAt": "datetime", 
  "modifiedAt": "datetime", 
  "isActive": true 
}
```
### Create Store
```
POST /api/stores
```
Creates a new store.

#### Request Body
```json
{ 
  "companyId": "guid", 
  "name": "string", 
  "location": "string" 
}
```
#### Response
```json
{ 
  "id": "guid", 
  "companyId": "guid", 
  "name": "string", 
  "location": "string", 
  "createdAt": "datetime", 
  "modifiedAt": null, 
  "isActive": true 
}
```
## Product API

### Get All Products
```
GET /api/products
```
Returns all active products.

#### Response
```json
[
  { 
    "id": "guid", 
    "name": "string", 
    "description": "string", 
    "price": 0.00, 
    "storeId": "guid",
    "createdAt": "datetime", 
    "modifiedAt": "datetime", 
    "isActive": true 
  } 
]
```
### Get Products by Store
```
GET /api/products/store/{storeId}
```
Returns all products for a specific store.

#### Parameters
- `storeId` (guid, required): The store's unique identifier

#### Response
```json
[
  { 
    "id": "guid", 
    "name": "string", 
    "description": "string", 
    "price": 0.00, 
    "storeId": "guid",
    "createdAt": "datetime", 
    "modifiedAt": "datetime", 
    "isActive": true 
  } 
]
```
### Create Product
```
POST /api/products
```

Creates a new product.

#### Request Body
```json
{ 
  "name": "string", 
  "description": "string", 
  "price": 0.00, 
  "storeId": "guid" 
}
```
#### Response
```json
{ 
  "id": "guid", 
  "name": "string", 
  "description": "string", 
  "price": 0.00, 
  "storeId": "guid",
  "createdAt": "datetime", 
  "modifiedAt": null, 
  "isActive": true 
}
```

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
```json
{ 
  "type": "ValidationError", 
  "message": "The request was invalid", 
  "errors": 
  { 
    "fieldName": [ "error message" ] 
  } 
}
```
### 401 Unauthorized
```json
{ 
  "type": "Unauthorized", 
  "message": "Authentication is required" 
}
```
### 404 Not Found
```json
{ 
  "type": "NotFoundError", 
  "message": "The requested resource was not found" 
}
```
## Rate Limiting
The API implements rate limiting of 100 requests per minute per client.

## Data Filtering
All collection endpoints support the following query parameters:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `active`: Filter by active status (true/false)

## Setup Guide

### Local Development
1. Clone the repository
2. Install .NET 8 SDK
3. Update connection string in `appsettings.json`
4. Run database migrations
5. Start the application

### Deployment
Refer to deployment documentation for staging and production environments.