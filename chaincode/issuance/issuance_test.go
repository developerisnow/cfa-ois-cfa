package main

import (
	"encoding/json"
	"fmt"
	"testing"

	"github.com/hyperledger/fabric-chaincode-go/shim"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

// MockTransactionContext wraps shim.MockStub for TransactionContextInterface
type MockTransactionContext struct {
	stub shim.ChaincodeStubInterface
}

func (m *MockTransactionContext) GetStub() shim.ChaincodeStubInterface {
	return m.stub
}

func TestIssue_HappyPath(t *testing.T) {
	contract := &IssuanceContract{}
	mockStub := shim.NewMockStub("issuance", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	id := "test-issuance-1"
	assetID := "asset-123"
	issuerID := "issuer-456"
	totalAmount := 1000000.0
	nominal := 1000.0
	issueDate := "2025-01-01"
	maturityDate := "2026-01-01"
	scheduleJSON := `{"items":[{"date":"2025-06-01","amount":50000}]}`

	err := contract.Issue(ctx, id, assetID, issuerID, totalAmount, nominal, issueDate, maturityDate, scheduleJSON)
	require.NoError(t, err)

	// Verify issuance was saved
	key := fmt.Sprintf("issuance:%s", id)
	state, err := ctx.stub.GetState(key)
	require.NoError(t, err)
	require.NotNil(t, state)

	var issuance Issuance
	err = json.Unmarshal(state, &issuance)
	require.NoError(t, err)

	assert.Equal(t, id, issuance.ID)
	assert.Equal(t, assetID, issuance.AssetID)
	assert.Equal(t, issuerID, issuance.IssuerID)
	assert.Equal(t, totalAmount, issuance.TotalAmount)
	assert.Equal(t, nominal, issuance.Nominal)
	assert.Equal(t, "published", issuance.Status)
	assert.Equal(t, 1, issuance.Version)
}

func TestIssue_Duplicate(t *testing.T) {
	contract := &IssuanceContract{}
	mockStub := shim.NewMockStub("issuance", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	id := "test-issuance-2"
	assetID := "asset-123"
	issuerID := "issuer-456"

	// First issue - should succeed
	err := contract.Issue(ctx, id, assetID, issuerID, 1000000.0, 1000.0, "2025-01-01", "2026-01-01", "")
	require.NoError(t, err)

	// Second issue with same ID - should fail
	err = contract.Issue(ctx, id, assetID, issuerID, 1000000.0, 1000.0, "2025-01-01", "2026-01-01", "")
	require.Error(t, err)
	assert.Contains(t, err.Error(), "already exists")
}

func TestClose_Flow(t *testing.T) {
	contract := &IssuanceContract{}
	mockStub := shim.NewMockStub("issuance", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	id := "test-issuance-3"
	assetID := "asset-123"
	issuerID := "issuer-456"

	// Create issuance
	err := contract.Issue(ctx, id, assetID, issuerID, 1000000.0, 1000.0, "2025-01-01", "2026-01-01", "")
	require.NoError(t, err)

	// Get before close
	issuance, err := contract.Get(ctx, id)
	require.NoError(t, err)
	assert.Equal(t, "published", issuance.Status)

	// Close issuance
	err = contract.Close(ctx, id)
	require.NoError(t, err)

	// Get after close
	issuance, err = contract.Get(ctx, id)
	require.NoError(t, err)
	assert.Equal(t, "closed", issuance.Status)
	assert.Equal(t, 2, issuance.Version) // Version should be incremented
}

func TestClose_InvalidStatus(t *testing.T) {
	contract := &IssuanceContract{}
	mockStub := shim.NewMockStub("issuance", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	id := "test-issuance-4"
	assetID := "asset-123"
	issuerID := "issuer-456"

	// Create issuance
	err := contract.Issue(ctx, id, assetID, issuerID, 1000000.0, 1000.0, "2025-01-01", "2026-01-01", "")
	require.NoError(t, err)

	// Close once
	err = contract.Close(ctx, id)
	require.NoError(t, err)

	// Try to close again - should fail
	err = contract.Close(ctx, id)
	require.Error(t, err)
	assert.Contains(t, err.Error(), "cannot close issuance in status")
}

func TestGet_NotFound(t *testing.T) {
	contract := &IssuanceContract{}
	mockStub := shim.NewMockStub("issuance", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	id := "non-existent-issuance"

	_, err := contract.Get(ctx, id)
	require.Error(t, err)
	assert.Contains(t, err.Error(), "does not exist")
}

func TestIssue_Validation(t *testing.T) {
	contract := &IssuanceContract{}
	mockStub := shim.NewMockStub("issuance", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	tests := []struct {
		name        string
		id          string
		assetID     string
		issuerID    string
		totalAmount float64
		nominal     float64
		expectError bool
		errorMsg    string
	}{
		{"empty id", "", "asset", "issuer", 1000, 100, true, "id is required"},
		{"empty assetID", "id", "", "issuer", 1000, 100, true, "assetID is required"},
		{"empty issuerID", "id", "asset", "", 1000, 100, true, "issuerID is required"},
		{"zero totalAmount", "id", "asset", "issuer", 0, 100, true, "totalAmount must be positive"},
		{"negative totalAmount", "id", "asset", "issuer", -1000, 100, true, "totalAmount must be positive"},
		{"zero nominal", "id", "asset", "issuer", 1000, 0, true, "nominal must be positive"},
		{"negative nominal", "id", "asset", "issuer", 1000, -100, true, "nominal must be positive"},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			err := contract.Issue(ctx, tt.id, tt.assetID, tt.issuerID, tt.totalAmount, tt.nominal, "2025-01-01", "2026-01-01", "")
			if tt.expectError {
				require.Error(t, err)
				assert.Contains(t, err.Error(), tt.errorMsg)
			} else {
				require.NoError(t, err)
			}
		})
	}
}

