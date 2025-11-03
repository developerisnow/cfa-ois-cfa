package main

import (
	"encoding/json"
	"fmt"
	"time"

	"github.com/hyperledger/fabric-contract-api-go/contractapi"
)

// RegistryContract provides functions for managing CFA transfers and redemptions
type RegistryContract struct {
	contractapi.Contract
}

// Transfer represents a transfer record on the ledger
type Transfer struct {
	ID           string    `json:"id"`
	From         *string   `json:"from,omitempty"` // null for issue
	To           string    `json:"to"`
	IssuanceID   string    `json:"issuanceId"`
	Amount       float64   `json:"amount"`
	TxHash       string    `json:"txHash"`
	Timestamp    time.Time `json:"timestamp"`
	Type         string    `json:"type"` // "transfer" or "issue"
}

// Holder represents a holder's balance for a specific issuance
type Holder struct {
	HolderID   string  `json:"holderId"`
	IssuanceID string  `json:"issuanceId"`
	Balance    float64 `json:"balance"`
}

// Transfer transfers CFA from one holder to another (or issues new)
func (rc *RegistryContract) Transfer(ctx contractapi.TransactionContextInterface, from string, to string, issuanceID string, amount float64) error {
	// Validate parameters
	if to == "" {
		return fmt.Errorf("to is required")
	}
	if issuanceID == "" {
		return fmt.Errorf("issuanceID is required")
	}
	if amount <= 0 {
		return fmt.Errorf("amount must be positive")
	}

	// Get transaction ID as transfer ID
	txID := ctx.GetStub().GetTxID()

	// If from is empty, this is an issue (initial transfer)
	var transferType string
	if from == "" {
		transferType = "issue"
	} else {
		transferType = "transfer"
		
		// Check sender balance
		senderKey := fmt.Sprintf("holder:%s:%s", from, issuanceID)
		senderJSON, err := ctx.GetStub().GetState(senderKey)
		if err != nil {
			return fmt.Errorf("failed to read sender state: %v", err)
		}
		
		var sender Holder
		if senderJSON != nil {
			if err := json.Unmarshal(senderJSON, &sender); err != nil {
				return fmt.Errorf("failed to unmarshal sender: %v", err)
			}
			if sender.Balance < amount {
				return fmt.Errorf("insufficient balance: has %f, need %f", sender.Balance, amount)
			}
			
			// Deduct from sender
			sender.Balance -= amount
			senderJSON, err = json.Marshal(sender)
			if err != nil {
				return fmt.Errorf("failed to marshal sender: %v", err)
			}
			if err := ctx.GetStub().PutState(senderKey, senderJSON); err != nil {
				return fmt.Errorf("failed to update sender state: %v", err)
			}
		} else {
			return fmt.Errorf("sender holder not found")
		}
	}

	// Add to receiver
	receiverKey := fmt.Sprintf("holder:%s:%s", to, issuanceID)
	receiverJSON, err := ctx.GetStub().GetState(receiverKey)
	var receiver Holder
	if receiverJSON != nil {
		if err := json.Unmarshal(receiverJSON, &receiver); err != nil {
			return fmt.Errorf("failed to unmarshal receiver: %v", err)
		}
	} else {
		receiver = Holder{
			HolderID:   to,
			IssuanceID: issuanceID,
			Balance:    0,
		}
	}

	receiver.Balance += amount
	receiverJSON, err = json.Marshal(receiver)
	if err != nil {
		return fmt.Errorf("failed to marshal receiver: %v", err)
	}
	if err := ctx.GetStub().PutState(receiverKey, receiverJSON); err != nil {
		return fmt.Errorf("failed to update receiver state: %v", err)
	}

	// Record transfer in history
	transfer := Transfer{
		ID:         txID,
		From:       &from,
		To:         to,
		IssuanceID: issuanceID,
		Amount:     amount,
		TxHash:     txID,
		Timestamp:  time.Now().UTC(),
		Type:       transferType,
	}
	if from == "" {
		transfer.From = nil
	}

	transferKey := fmt.Sprintf("transfer:%s", txID)
	transferJSON, err := json.Marshal(transfer)
	if err != nil {
		return fmt.Errorf("failed to marshal transfer: %v", err)
	}
	if err := ctx.GetStub().PutState(transferKey, transferJSON); err != nil {
		return fmt.Errorf("failed to record transfer: %v", err)
	}

	// Add to issuance history index
	historyKey := fmt.Sprintf("history:%s:%s", issuanceID, txID)
	if err := ctx.GetStub().PutState(historyKey, transferJSON); err != nil {
		return fmt.Errorf("failed to record history: %v", err)
	}

	return nil
}

// Redeem redeems CFA from a holder
func (rc *RegistryContract) Redeem(ctx contractapi.TransactionContextInterface, holderID string, issuanceID string, amount float64) error {
	// Validate parameters
	if holderID == "" {
		return fmt.Errorf("holderID is required")
	}
	if issuanceID == "" {
		return fmt.Errorf("issuanceID is required")
	}
	if amount <= 0 {
		return fmt.Errorf("amount must be positive")
	}

	// Get holder balance
	holderKey := fmt.Sprintf("holder:%s:%s", holderID, issuanceID)
	holderJSON, err := ctx.GetStub().GetState(holderKey)
	if err != nil {
		return fmt.Errorf("failed to read holder state: %v", err)
	}
	if holderJSON == nil {
		return fmt.Errorf("holder not found")
	}

	var holder Holder
	if err := json.Unmarshal(holderJSON, &holder); err != nil {
		return fmt.Errorf("failed to unmarshal holder: %v", err)
	}

	if holder.Balance < amount {
		return fmt.Errorf("insufficient balance: has %f, need %f", holder.Balance, amount)
	}

	// Deduct balance
	holder.Balance -= amount
	holderJSON, err = json.Marshal(holder)
	if err != nil {
		return fmt.Errorf("failed to marshal holder: %v", err)
	}
	if err := ctx.GetStub().PutState(holderKey, holderJSON); err != nil {
		return fmt.Errorf("failed to update holder state: %v", err)
	}

	// Record redemption in history
	txID := ctx.GetStub().GetTxID()
	transfer := Transfer{
		ID:         txID,
		From:       &holderID,
		To:         "",
		IssuanceID: issuanceID,
		Amount:     amount,
		TxHash:     txID,
		Timestamp:  time.Now().UTC(),
		Type:       "redeem",
	}

	transferKey := fmt.Sprintf("transfer:%s", txID)
	transferJSON, err := json.Marshal(transfer)
	if err != nil {
		return fmt.Errorf("failed to marshal transfer: %v", err)
	}
	if err := ctx.GetStub().PutState(transferKey, transferJSON); err != nil {
		return fmt.Errorf("failed to record transfer: %v", err)
	}

	historyKey := fmt.Sprintf("history:%s:%s", issuanceID, txID)
	if err := ctx.GetStub().PutState(historyKey, transferJSON); err != nil {
		return fmt.Errorf("failed to record history: %v", err)
	}

	return nil
}

// GetHistory returns transfer history for an issuance
func (rc *RegistryContract) GetHistory(ctx contractapi.TransactionContextInterface, issuanceID string) ([]*Transfer, error) {
	if issuanceID == "" {
		return nil, fmt.Errorf("issuanceID is required")
	}

	// Query history index using partial key match
	historyPrefix := fmt.Sprintf("history:%s:", issuanceID)
	historyIterator, err := ctx.GetStub().GetStateByRange(historyPrefix, historyPrefix+"\xff")
	if err != nil {
		return nil, fmt.Errorf("failed to get history: %v", err)
	}
	defer historyIterator.Close()

	var history []*Transfer
	for historyIterator.HasNext() {
		response, err := historyIterator.Next()
		if err != nil {
			return nil, fmt.Errorf("failed to iterate history: %v", err)
		}

		var transfer Transfer
		if err := json.Unmarshal(response.Value, &transfer); err != nil {
			return nil, fmt.Errorf("failed to unmarshal transfer: %v", err)
		}

		history = append(history, &transfer)
	}

	return history, nil
}

func main() {
	registryContract, err := contractapi.NewChaincode(&RegistryContract{})
	if err != nil {
		panic(fmt.Sprintf("Error creating chaincode: %v", err))
	}

	if err := registryContract.Start(); err != nil {
		panic(fmt.Sprintf("Error starting chaincode: %v", err))
	}
}

