# API Testing Guide

## Prerequisites
- [Postman](https://www.postman.com/downloads/)
- Access to environment credentials

## Setup
1. Import the Postman collection from `tests/Postman/QuartileChallenge.postman_collection.json`
2. Import environments from `tests/Postman/environments/`
3. Set environment variables:
   - `baseUrl`: API base URL
   - `storeId`: Valid store ID for testing
   - `companyId`: Valid company ID for testing

## Running Tests
1. Select the appropriate environment (Local/Staging/Production)
2. Run the entire collection or individual requests
3. View test results in Postman's test results tab

## Test Coverage
The test suite covers:
- Store CRUD operations
- Product CRUD operations
- Multi-company filtering
- Error handling
- Authentication
- Rate limiting

## Adding New Tests
1. Create a new request in the appropriate folder
2. Add test scripts using Postman's test tab
3. Document the test purpose and assertions
4. Update the collection file in source control