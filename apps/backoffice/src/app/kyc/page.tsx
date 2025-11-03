'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { useState } from 'react';

export default function KycPage() {
  const [investorId, setInvestorId] = useState('');

  const { data: status, isLoading } = useQuery({
    queryKey: ['investor-status', investorId],
    queryFn: async () => {
      if (!investorId) return null;
      const response = await apiClient.getInvestorStatus({ id: investorId });
      return response.data;
    },
    enabled: !!investorId,
  });

  return (
    <div className="container mx-auto p-8">
      <h1 className="text-3xl font-bold mb-6">KYC Management</h1>

      <div className="mb-6">
        <input
          type="text"
          placeholder="Enter Investor ID"
          value={investorId}
          onChange={(e) => setInvestorId(e.target.value)}
          className="px-4 py-2 border rounded-lg w-96"
        />
      </div>

      {isLoading && <div>Loading...</div>}

      {status && (
        <div className="bg-white p-6 rounded-lg shadow">
          <h2 className="text-xl font-semibold mb-4">Investor Status</h2>
          <div className="space-y-2">
            <p><strong>KYC:</strong> {status.kyc}</p>
            <p><strong>Qualification Tier:</strong> {status.qualificationTier}</p>
            <p><strong>Qualification Limit:</strong> {status.qualificationLimit || 'N/A'}</p>
            <p><strong>Qualification Used:</strong> {status.qualificationUsed || 0}</p>
          </div>
        </div>
      )}
    </div>
  );
}

