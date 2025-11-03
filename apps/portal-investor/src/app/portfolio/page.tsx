'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';

export default function PortfolioPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [investorId] = useState('3fa85f64-5717-4562-b3fc-2c963f66afa6'); // TODO: Get from session

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: wallet, isLoading } = useQuery({
    queryKey: ['wallet', investorId],
    queryFn: async () => {
      const response = await apiClient.getWallet(investorId);
      return response.data;
    },
    enabled: !!investorId && status === 'authenticated' && !!session,
  });

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  return (
    <div className="container mx-auto p-8">
      <h1 className="text-3xl font-bold mb-6">Portfolio</h1>

      {isLoading && <div>Loading portfolio...</div>}

      {wallet && (
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-lg shadow">
            <h2 className="text-xl font-semibold mb-4">Wallet</h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-gray-600">Balance</p>
                <p className="text-2xl font-bold">{wallet.balance || 0}</p>
              </div>
              <div>
                <p className="text-gray-600">Blocked</p>
                <p className="text-2xl font-bold">{wallet.blocked || 0}</p>
              </div>
            </div>
          </div>

          <div className="bg-white p-6 rounded-lg shadow">
            <h2 className="text-xl font-semibold mb-4">Holdings</h2>
            {wallet.holdings && wallet.holdings.length > 0 ? (
              <div className="space-y-4">
                {wallet.holdings.map((holding: any) => (
                  <div key={holding.issuanceId} className="border-b pb-4">
                    <p className="font-semibold">Issuance: {holding.issuanceId}</p>
                    <p className="text-gray-600">Quantity: {holding.quantity}</p>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500">No holdings yet</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

