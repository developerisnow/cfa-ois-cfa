package main

import (
	"encoding/json"
	"fmt"
	"time"

	"github.com/hyperledger/fabric-contract-api-go/contractapi"
)

// IssuanceContract provides functions for managing issuances
type IssuanceContract struct {
	contractapi.Contract
}

// Issuance represents an issuance on the ledger
type Issuance struct {
	ID           string                 `json:"id"`
	AssetID      string                 `json:"assetId"`
	IssuerID     string                 `json:"issuerId"`
	TotalAmount  float64                `json:"totalAmount"`
	Nominal      float64                `json:"nominal"`
	IssueDate    string                 `json:"issueDate"`
	MaturityDate string                 `json:"maturityDate"`
	Status       string                 `json:"status"`
	Schedule     map[string]interface{} `json:"schedule,omitempty"`
	Version      int                    `json:"version"`
	CreatedAt    string                 `json:"createdAt"`
	UpdatedAt    string                 `json:"updatedAt"`
}

// Issue creates a new issuance on the ledger
func (ic *IssuanceContract) Issue(ctx contractapi.TransactionContextInterface, id string, assetID string, issuerID string, totalAmount float64, nominal float64, issueDate string, maturityDate string, scheduleJSON string) error {
	// Validate parameters
	if id == "" {
		return fmt.Errorf("id is required")
	}
	if assetID == "" {
		return fmt.Errorf("assetID is required")
	}
	if issuerID == "" {
		return fmt.Errorf("issuerID is required")
	}
	if totalAmount <= 0 {
		return fmt.Errorf("totalAmount must be positive")
	}
	if nominal <= 0 {
		return fmt.Errorf("nominal must be positive")
	}

	// Check if issuance already exists
	key := fmt.Sprintf("issuance:%s", id)
	existing, err := ctx.GetStub().GetState(key)
	if err != nil {
		return fmt.Errorf("failed to read from world state: %v", err)
	}
	if existing != nil {
		return fmt.Errorf("issuance %s already exists", id)
	}

	// Parse schedule if provided
	var schedule map[string]interface{}
	if scheduleJSON != "" {
		if err := json.Unmarshal([]byte(scheduleJSON), &schedule); err != nil {
			return fmt.Errorf("invalid scheduleJSON: %v", err)
		}
	}

	// Create issuance
	now := time.Now().UTC().Format(time.RFC3339)
	issuance := Issuance{
		ID:           id,
		AssetID:      assetID,
		IssuerID:     issuerID,
		TotalAmount:  totalAmount,
		Nominal:      nominal,
		IssueDate:    issueDate,
		MaturityDate: maturityDate,
		Status:       "published",
		Schedule:     schedule,
		Version:      1,
		CreatedAt:    now,
		UpdatedAt:    now,
	}

	issuanceJSON, err := json.Marshal(issuance)
	if err != nil {
		return fmt.Errorf("failed to marshal issuance: %v", err)
	}

	// Save to ledger
	return ctx.GetStub().PutState(key, issuanceJSON)
}

// Close closes an issuance by updating its status
func (ic *IssuanceContract) Close(ctx contractapi.TransactionContextInterface, id string) error {
	key := fmt.Sprintf("issuance:%s", id)

	// Get issuance
	issuanceJSON, err := ctx.GetStub().GetState(key)
	if err != nil {
		return fmt.Errorf("failed to read from world state: %v", err)
	}
	if issuanceJSON == nil {
		return fmt.Errorf("issuance %s does not exist", id)
	}

	// Unmarshal
	var issuance Issuance
	if err := json.Unmarshal(issuanceJSON, &issuance); err != nil {
		return fmt.Errorf("failed to unmarshal issuance: %v", err)
	}

	// Validate status
	if issuance.Status != "published" {
		return fmt.Errorf("cannot close issuance in status %s", issuance.Status)
	}

	// Update status
	issuance.Status = "closed"
	issuance.UpdatedAt = time.Now().UTC().Format(time.RFC3339)
	issuance.Version++

	// Save back to ledger
	updatedJSON, err := json.Marshal(issuance)
	if err != nil {
		return fmt.Errorf("failed to marshal updated issuance: %v", err)
	}

	return ctx.GetStub().PutState(key, updatedJSON)
}

// Get returns the issuance details
func (ic *IssuanceContract) Get(ctx contractapi.TransactionContextInterface, id string) (*Issuance, error) {
	key := fmt.Sprintf("issuance:%s", id)

	issuanceJSON, err := ctx.GetStub().GetState(key)
	if err != nil {
		return nil, fmt.Errorf("failed to read from world state: %v", err)
	}
	if issuanceJSON == nil {
		return nil, fmt.Errorf("issuance %s does not exist", id)
	}

	var issuance Issuance
	if err := json.Unmarshal(issuanceJSON, &issuance); err != nil {
		return nil, fmt.Errorf("failed to unmarshal issuance: %v", err)
	}

	return &issuance, nil
}

func main() {
	issuanceContract, err := contractapi.NewChaincode(&IssuanceContract{})
	if err != nil {
		panic(fmt.Sprintf("Error creating chaincode: %v", err))
	}

	if err := issuanceContract.Start(); err != nil {
		panic(fmt.Sprintf("Error starting chaincode: %v", err))
	}
}

