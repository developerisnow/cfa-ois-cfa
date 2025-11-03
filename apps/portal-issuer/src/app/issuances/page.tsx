'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import Link from 'next/link';
import { useSession } from 'next-auth/react';
import { redirect } from 'next/navigation';

export default function IssuancesPage() {
  const { data: session, status } = useSession();

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    redirect('/auth/signin');
  }

  const { data: issuances, isLoading, error } = useQuery({
    queryKey: ['issuances'],
    queryFn: async () => {
      // TODO: Add endpoint to list issuances
      return [];
    },
  });

  return (
    <div className="container mx-auto p-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Issuances</h1>
        <Link
          href="/issuances/create"
          className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700"
        >
          Create New
        </Link>
      </div>

      {isLoading && <div>Loading issuances...</div>}
      {error && <div className="text-red-600">Error loading issuances</div>}
      
      {issuances && issuances.length === 0 && (
        <div className="text-gray-500">No issuances found. Create your first issuance.</div>
      )}

      {issuances && issuances.length > 0 && (
        <div className="space-y-4">
          {issuances.map((issuance: any) => (
            <div key={issuance.id} className="bg-white p-6 rounded-lg shadow">
              <div className="flex justify-between items-center">
                <div>
                  <h2 className="text-xl font-semibold">Issuance {issuance.id}</h2>
                  <p className="text-gray-600">Status: {issuance.status}</p>
                  <p className="text-gray-600">Amount: {issuance.totalAmount}</p>
                </div>
                <div className="space-x-2">
                  <Link
                    href={`/issuances/${issuance.id}`}
                    className="bg-gray-200 text-gray-800 px-4 py-2 rounded hover:bg-gray-300"
                  >
                    View
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

