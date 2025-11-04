import { test, expect } from '@playwright/test';

/**
 * E2E тест для проверки интеграции с Hyperledger Fabric dev-сетью
 * 
 * Требования:
 * 1. Fabric сеть запущена: cd ops/fabric && ./dev-up.sh && ./scripts/create-channel.sh && ./scripts/install-chaincode.sh && ./scripts/approve-chaincode.sh
 * 2. Сервисы запущены с Ledger:UseMock=false
 * 3. Chaincode Gateway доступен на http://localhost:8080 (или настроен endpoint)
 */
test.describe('Ledger Integration E2E', () => {
  test.beforeAll(async () => {
    // Проверка доступности Fabric сети
    // Можно добавить health-check через API или docker exec
  });

  test('Publish issuance → Verify txHash returned', async ({ request }) => {
    // 1. Создать issuance (draft)
    const createResponse = await request.post('http://localhost:5001/v1/issuances', {
      data: {
        assetId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        issuerId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        totalAmount: 1000000,
        nominal: 1000,
        issueDate: '2025-01-01',
        maturityDate: '2026-01-01',
      },
    });

    expect(createResponse.ok()).toBeTruthy();
    const issuance = await createResponse.json();
    const issuanceId = issuance.id;

    // 2. Publish issuance (должен вызвать ledger Issue)
    const publishResponse = await request.post(
      `http://localhost:5001/v1/issuances/${issuanceId}/publish`
    );

    expect(publishResponse.ok()).toBeTruthy();
    const publishResult = await publishResponse.json();

    // 3. Verify txHash returned
    expect(publishResult).toHaveProperty('dltTxHash');
    expect(publishResult.dltTxHash).toBeTruthy();
    expect(typeof publishResult.dltTxHash).toBe('string');
    expect(publishResult.dltTxHash.length).toBeGreaterThan(0);

    // 4. Query issuance from ledger (optional - через chaincode query)
    const getResponse = await request.get(`http://localhost:5001/v1/issuances/${issuanceId}`);
    expect(getResponse.ok()).toBeTruthy();
    const issuanceData = await getResponse.json();
    expect(issuanceData.dltTxHash).toBe(publishResult.dltTxHash);
  });

  test('Place order → Transfer on ledger → Verify txHash', async ({ request }) => {
    // Предполагаем, что issuance уже опубликован (из предыдущего теста или setup)
    const issuanceId = '3fa85f64-5717-4562-b3fc-2c963f66afa6'; // TODO: использовать реальный ID
    const investorId = '3fa85f64-5717-4562-b3fc-2c963f66afa7';

    // 1. Place order (должен вызвать ledger Transfer)
    const orderResponse = await request.post('http://localhost:5002/v1/orders', {
      headers: {
        'Idempotency-Key': crypto.randomUUID(),
      },
      data: {
        investorId,
        issuanceId,
        amount: 10000,
      },
    });

    expect(orderResponse.ok() || orderResponse.status() === 202).toBeTruthy();
    const order = await orderResponse.json();

    // 2. Verify txHash returned
    expect(order).toHaveProperty('dltTxHash');
    expect(order.dltTxHash).toBeTruthy();
    expect(typeof order.dltTxHash).toBe('string');

    // 3. Verify order status
    const orderId = order.id;
    const getOrderResponse = await request.get(`http://localhost:5002/v1/orders/${orderId}`);
    expect(getOrderResponse.ok()).toBeTruthy();
    const orderData = await getOrderResponse.json();
    expect(orderData.dltTxHash).toBe(order.dltTxHash);
  });

  test('Redeem → Verify txHash returned', async ({ request }) => {
    const issuanceId = '3fa85f64-5717-4562-b3fc-2c963f66afa6'; // TODO: использовать реальный ID
    const holderId = '3fa85f64-5717-4562-b3fc-2c963f66afa7';

    // 1. Redeem (должен вызвать ledger Redeem)
    const redeemResponse = await request.post(
      `http://localhost:5002/v1/issuances/${issuanceId}/redeem`,
      {
        data: {
          amount: 5000,
        },
      }
    );

    expect(redeemResponse.ok()).toBeTruthy();
    const redeemResult = await redeemResponse.json();

    // 2. Verify txHash returned
    expect(redeemResult).toHaveProperty('dltTxHash');
    expect(redeemResult.dltTxHash).toBeTruthy();
    expect(typeof redeemResult.dltTxHash).toBe('string');
  });

  test('Full lifecycle: Issue → Transfer → Redeem', async ({ request }) => {
    // 1. Create and publish issuance
    const createResponse = await request.post('http://localhost:5001/v1/issuances', {
      data: {
        assetId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        issuerId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        totalAmount: 1000000,
        nominal: 1000,
        issueDate: '2025-01-01',
        maturityDate: '2026-01-01',
      },
    });

    const issuance = await createResponse.json();
    const issuanceId = issuance.id;

    const publishResponse = await request.post(
      `http://localhost:5001/v1/issuances/${issuanceId}/publish`
    );
    const publishResult = await publishResponse.json();
    const issueTxHash = publishResult.dltTxHash;

    expect(issueTxHash).toBeTruthy();
    console.log(`Issuance published with txHash: ${issueTxHash}`);

    // 2. Place order (Transfer)
    const investorId = '3fa85f64-5717-4562-b3fc-2c963f66afa7';
    const orderResponse = await request.post('http://localhost:5002/v1/orders', {
      headers: {
        'Idempotency-Key': crypto.randomUUID(),
      },
      data: {
        investorId,
        issuanceId,
        amount: 50000,
      },
    });

    const order = await orderResponse.json();
    const transferTxHash = order.dltTxHash;

    expect(transferTxHash).toBeTruthy();
    expect(transferTxHash).not.toBe(issueTxHash);
    console.log(`Transfer executed with txHash: ${transferTxHash}`);

    // 3. Redeem
    const redeemResponse = await request.post(
      `http://localhost:5002/v1/issuances/${issuanceId}/redeem`,
      {
        data: {
          amount: 10000,
        },
      }
    );

    const redeemResult = await redeemResponse.json();
    const redeemTxHash = redeemResult.dltTxHash;

    expect(redeemTxHash).toBeTruthy();
    expect(redeemTxHash).not.toBe(issueTxHash);
    expect(redeemTxHash).not.toBe(transferTxHash);
    console.log(`Redeem executed with txHash: ${redeemTxHash}`);

    // 4. Verify all txHash are unique
    expect(issueTxHash).not.toBe(transferTxHash);
    expect(transferTxHash).not.toBe(redeemTxHash);
    expect(issueTxHash).not.toBe(redeemTxHash);
  });
});

