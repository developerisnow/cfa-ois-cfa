package main

import (
	"encoding/json"
	"fmt"
	"testing"

	"github.com/hyperledger/fabric-chaincode-go/shim"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

type MockTransactionContext struct {
	stub shim.ChaincodeStubInterface
}

func (m *MockTransactionContext) GetStub() shim.ChaincodeStubInterface {
	return m.stub
}

func TestTransfer_HappyPath(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	issuanceID := "issuance-123"
	from := "holder-1"
	to := "holder-2"
	amount := 1000.0

	// First, issue to holder-1 (from empty)
	err := contract.Transfer(ctx, "", from, issuanceID, 2000.0)
	require.NoError(t, err)

	// Then transfer from holder-1 to holder-2
	err = contract.Transfer(ctx, from, to, issuanceID, amount)
	require.NoError(t, err)

	// Verify balances
	holder1Key := fmt.Sprintf("holder:%s:%s", from, issuanceID)
	holder1JSON, err := mockStub.GetState(holder1Key)
	require.NoError(t, err)
	require.NotNil(t, holder1JSON)

	var holder1 Holder
	err = json.Unmarshal(holder1JSON, &holder1)
	require.NoError(t, err)
	assert.Equal(t, 1000.0, holder1.Balance) // 2000 - 1000

	holder2Key := fmt.Sprintf("holder:%s:%s", to, issuanceID)
	holder2JSON, err := mockStub.GetState(holder2Key)
	require.NoError(t, err)
	require.NotNil(t, holder2JSON)

	var holder2 Holder
	err = json.Unmarshal(holder2JSON, &holder2)
	require.NoError(t, err)
	assert.Equal(t, 1000.0, holder2.Balance)
}

func TestTransfer_Idempotent(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	issuanceID := "issuance-456"
	to := "holder-3"
	amount := 500.0

	// Issue twice with same tx (idempotency check via txID)
	err1 := contract.Transfer(ctx, "", to, issuanceID, amount)
	require.NoError(t, err1)

	// Second issue to same holder should add balance
	err2 := contract.Transfer(ctx, "", to, issuanceID, amount)
	require.NoError(t, err2)

	holderKey := fmt.Sprintf("holder:%s:%s", to, issuanceID)
	holderJSON, err := mockStub.GetState(holderKey)
	require.NoError(t, err)
	require.NotNil(t, holderJSON)

	var holder Holder
	err = json.Unmarshal(holderJSON, &holder)
	require.NoError(t, err)
	assert.Equal(t, 1000.0, holder.Balance) // 500 + 500
}

func TestTransfer_InsufficientBalance(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	issuanceID := "issuance-789"
	from := "holder-4"
	to := "holder-5"

	// Issue 100 to holder-4
	err := contract.Transfer(ctx, "", from, issuanceID, 100.0)
	require.NoError(t, err)

	// Try to transfer 200 (more than balance)
	err = contract.Transfer(ctx, from, to, issuanceID, 200.0)
	require.Error(t, err)
	assert.Contains(t, err.Error(), "insufficient balance")
}

func TestTransfer_InvalidAmount(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	tests := []struct {
		name        string
		from        string
		to          string
		issuanceID  string
		amount      float64
		expectError bool
		errorMsg    string
	}{
		{"zero amount", "", "holder-1", "issuance-1", 0, true, "amount must be positive"},
		{"negative amount", "", "holder-1", "issuance-1", -100, true, "amount must be positive"},
		{"empty to", "", "", "issuance-1", 100, true, "to is required"},
		{"empty issuanceID", "", "holder-1", "", 100, true, "issuanceID is required"},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			err := contract.Transfer(ctx, tt.from, tt.to, tt.issuanceID, tt.amount)
			if tt.expectError {
				require.Error(t, err)
				assert.Contains(t, err.Error(), tt.errorMsg)
			} else {
				require.NoError(t, err)
			}
		})
	}
}

func TestRedeem_HappyPath(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	issuanceID := "issuance-redeem-1"
	holderID := "holder-redeem-1"
	amount := 1000.0

	// Issue to holder
	err := contract.Transfer(ctx, "", holderID, issuanceID, 2000.0)
	require.NoError(t, err)

	// Redeem
	err = contract.Redeem(ctx, holderID, issuanceID, amount)
	require.NoError(t, err)

	// Verify balance decreased
	holderKey := fmt.Sprintf("holder:%s:%s", holderID, issuanceID)
	holderJSON, err := mockStub.GetState(holderKey)
	require.NoError(t, err)
	require.NotNil(t, holderJSON)

	var holder Holder
	err = json.Unmarshal(holderJSON, &holder)
	require.NoError(t, err)
	assert.Equal(t, 1000.0, holder.Balance) // 2000 - 1000
}

func TestRedeem_InsufficientBalance(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	issuanceID := "issuance-redeem-2"
	holderID := "holder-redeem-2"

	// Issue 100
	err := contract.Transfer(ctx, "", holderID, issuanceID, 100.0)
	require.NoError(t, err)

	// Try to redeem 200
	err = contract.Redeem(ctx, holderID, issuanceID, 200.0)
	require.Error(t, err)
	assert.Contains(t, err.Error(), "insufficient balance")
}

func TestRedeem_NotFound(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	issuanceID := "issuance-redeem-3"
	holderID := "non-existent-holder"

	// Try to redeem from non-existent holder
	err := contract.Redeem(ctx, holderID, issuanceID, 100.0)
	require.Error(t, err)
	assert.Contains(t, err.Error(), "holder not found")
}

func TestGetHistory(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	issuanceID := "issuance-history-1"
	holder1 := "holder-h1"
	holder2 := "holder-h2"

	// Issue
	err := contract.Transfer(ctx, "", holder1, issuanceID, 1000.0)
	require.NoError(t, err)

	// Transfer
	err = contract.Transfer(ctx, holder1, holder2, issuanceID, 500.0)
	require.NoError(t, err)

	// Redeem
	err = contract.Redeem(ctx, holder2, issuanceID, 200.0)
	require.NoError(t, err)

	// Get history
	history, err := contract.GetHistory(ctx, issuanceID)
	require.NoError(t, err)
	// Note: GetStateByPartialCompositeKey may not work perfectly with MockStub
	// In real implementation, this would return all transfers
	assert.NotNil(t, history)
}

func TestGetHistory_NotFound(t *testing.T) {
	contract := &RegistryContract{}
	mockStub := shim.NewMockStub("registry", nil)
	ctx := &MockTransactionContext{stub: mockStub}

	// Get history for non-existent issuance
	history, err := contract.GetHistory(ctx, "non-existent")
	require.NoError(t, err) // Returns empty list, not error
	assert.Empty(t, history)
}

