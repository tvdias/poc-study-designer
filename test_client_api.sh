#!/bin/bash

# POC Study Designer - Client API Test Script
# This script demonstrates all CRUD operations for the Client Management feature

# Configuration
API_BASE_URL="http://localhost:5000/api"
CREATED_CLIENT_ID=""

echo "=========================================="
echo "POC Study Designer - Client API Tests"
echo "=========================================="
echo ""
echo "Base URL: $API_BASE_URL"
echo ""

# Helper function to print test results
print_result() {
    echo ""
    echo "--- $1 ---"
    echo "HTTP Status: $HTTP_STATUS"
    echo "Response:"
    echo "$RESPONSE" | jq '.' 2>/dev/null || echo "$RESPONSE"
    echo ""
}

# Test 1: Create a Client
echo "TEST 1: Create a Client (POST /api/clients)"
echo "Request Body:"
echo '{
  "name": "Acme Corporation",
  "integrationMetadata": "API_KEY=12345",
  "productsModules": "Product A, Product B"
}'
echo ""

RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_BASE_URL/clients" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Acme Corporation",
    "integrationMetadata": "API_KEY=12345",
    "productsModules": "Product A, Product B"
  }')

HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
RESPONSE=$(echo "$RESPONSE" | head -n-1)
print_result "Create Client Response"

# Extract the client ID for subsequent tests
CREATED_CLIENT_ID=$(echo "$RESPONSE" | jq -r '.id' 2>/dev/null)
echo "Extracted Client ID: $CREATED_CLIENT_ID"
echo ""

# Test 2: Get All Clients
echo "TEST 2: Get All Clients (GET /api/clients)"
echo ""

RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE_URL/clients")

HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
RESPONSE=$(echo "$RESPONSE" | head -n-1)
print_result "Get All Clients Response"

# Test 3: Get All Clients with Search Filter
echo "TEST 3: Get All Clients with Search Filter (GET /api/clients?query=Acme)"
echo ""

RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE_URL/clients?query=Acme")

HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
RESPONSE=$(echo "$RESPONSE" | head -n-1)
print_result "Get Clients Filtered Response"

# Test 4: Get Client by ID
if [ ! -z "$CREATED_CLIENT_ID" ] && [ "$CREATED_CLIENT_ID" != "null" ]; then
    echo "TEST 4: Get Client by ID (GET /api/clients/$CREATED_CLIENT_ID)"
    echo ""

    RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE_URL/clients/$CREATED_CLIENT_ID")

    HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
    RESPONSE=$(echo "$RESPONSE" | head -n-1)
    print_result "Get Client by ID Response"

    # Test 5: Update Client
    echo "TEST 5: Update Client (PUT /api/clients/$CREATED_CLIENT_ID)"
    echo "Request Body:"
    echo '{
  "name": "Acme Corporation Updated",
  "integrationMetadata": "API_KEY=67890",
  "productsModules": "Product A, Product C",
  "isActive": true
}'
    echo ""

    RESPONSE=$(curl -s -w "\n%{http_code}" -X PUT "$API_BASE_URL/clients/$CREATED_CLIENT_ID" \
      -H "Content-Type: application/json" \
      -d '{
        "name": "Acme Corporation Updated",
        "integrationMetadata": "API_KEY=67890",
        "productsModules": "Product A, Product C",
        "isActive": true
      }')

    HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
    RESPONSE=$(echo "$RESPONSE" | head -n-1)
    print_result "Update Client Response"

    # Test 6: Delete Client
    echo "TEST 6: Delete Client (DELETE /api/clients/$CREATED_CLIENT_ID)"
    echo ""

    RESPONSE=$(curl -s -w "\n%{http_code}" -X DELETE "$API_BASE_URL/clients/$CREATED_CLIENT_ID")

    HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
    RESPONSE=$(echo "$RESPONSE" | head -n-1)
    if [ -z "$RESPONSE" ]; then
        RESPONSE="<No content returned>"
    fi
    print_result "Delete Client Response"

    # Test 7: Verify Deletion (Get should return 404)
    echo "TEST 7: Verify Deletion (GET /api/clients/$CREATED_CLIENT_ID should return 404)"
    echo ""

    RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE_URL/clients/$CREATED_CLIENT_ID")

    HTTP_STATUS=$(echo "$RESPONSE" | tail -n1)
    RESPONSE=$(echo "$RESPONSE" | head -n-1)
    print_result "Get Deleted Client Response (Should be 404)"
else
    echo "ERROR: Could not extract client ID from create response. Skipping remaining tests."
fi

echo "=========================================="
echo "Test Suite Complete"
echo "=========================================="
