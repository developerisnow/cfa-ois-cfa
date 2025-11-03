'use client';

import { useQuery, useMutation } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, EmptyState, Skeleton } from '../../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import Link from 'next/link';

export default function NewOrderPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [formData, setFormData] = useState({
    clientId: '',
    issuanceId: '',
    amount: '',
  });

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: clientsData, isLoading: clientsLoading } = useQuery({
    queryKey: ['broker-clients'],
    queryFn: async () => {
      const response = await apiClient.getBrokerClients({ limit: 1000 });
      return response.data;
    },
    enabled: status === 'authenticated',
  });

  const { data: issuancesData, isLoading: issuancesLoading } = useQuery({
    queryKey: ['market-issuances'],
    queryFn: async () => {
      const response = await apiClient.getMarketIssuances({ status: 'open', limit: 100 });
      return response.data;
    },
    enabled: status === 'authenticated',
  });

  const orderMutation = useMutation({
    mutationFn: async (data: { clientId: string; issuanceId: string; amount: number }) => {
      const response = await apiClient.createBrokerOrder(data, {
        headers: {
          'Idempotency-Key': crypto.randomUUID(),
        },
      });
      return response.data;
    },
    onSuccess: () => {
      toast.success('Order created successfully');
      router.push('/feed');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to create order');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const amount = parseFloat(formData.amount);
    if (!amount || amount <= 0) {
      toast.error('Please enter a valid amount');
      return;
    }
    if (!formData.clientId || !formData.issuanceId) {
      toast.error('Please select client and issuance');
      return;
    }
    orderMutation.mutate({
      clientId: formData.clientId,
      issuanceId: formData.issuanceId,
      amount,
    });
  };

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  return (
    <AppShell
      user={session.user}
      sidebar={{
        items: [
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Clients', href: '/clients' },
          { label: 'New Order', href: '/orders/new' },
          { label: 'Feed', href: '/feed' },
        ],
      }}
    >
      <PageHeader
        title="Create Order"
        description="Create order on behalf of client"
        breadcrumbs={[
          { label: 'Orders', href: '/orders' },
          { label: 'New' },
        ]}
        actions={
          <Link
            href="/dashboard"
            className="flex items-center gap-2 px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover"
          >
            <ArrowLeft className="h-4 w-4" />
            Back
          </Link>
        }
      />

      <div className="max-w-2xl mx-auto">
        <form onSubmit={handleSubmit} className="bg-surface border border-border rounded-lg p-6 space-y-6">
          <div>
            <label htmlFor="clientId" className="block text-sm font-medium text-text-primary mb-2">
              Client *
            </label>
            {clientsLoading ? (
              <Skeleton className="h-10 w-full" variant="rectangular" />
            ) : (
              <select
                id="clientId"
                value={formData.clientId}
                onChange={(e) => setFormData({ ...formData, clientId: e.target.value })}
                className="w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
                required
              >
                <option value="">Select client...</option>
                {clientsData?.items?.map((client: any) => (
                  <option key={client.id} value={client.id}>
                    {client.name} ({client.email})
                  </option>
                ))}
              </select>
            )}
          </div>

          <div>
            <label htmlFor="issuanceId" className="block text-sm font-medium text-text-primary mb-2">
              Issuance *
            </label>
            {issuancesLoading ? (
              <Skeleton className="h-10 w-full" variant="rectangular" />
            ) : (
              <select
                id="issuanceId"
                value={formData.issuanceId}
                onChange={(e) => setFormData({ ...formData, issuanceId: e.target.value })}
                className="w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
                required
              >
                <option value="">Select issuance...</option>
                {issuancesData?.items?.map((issuance: any) => (
                  <option key={issuance.id} value={issuance.id}>
                    {issuance.assetName} - {issuance.yield}% yield
                  </option>
                ))}
              </select>
            )}
          </div>

          <div>
            <label htmlFor="amount" className="block text-sm font-medium text-text-primary mb-2">
              Amount (₽) *
            </label>
            <input
              id="amount"
              type="number"
              step="0.01"
              min="0"
              value={formData.amount}
              onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
              className="w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
              required
              placeholder="0.00"
            />
          </div>

          <div className="flex justify-end gap-4">
            <Link
              href="/dashboard"
              className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover"
            >
              Cancel
            </Link>
            <button
              type="submit"
              disabled={orderMutation.isPending}
              className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {orderMutation.isPending ? 'Creating...' : 'Create Order'}
            </button>
          </div>
        </form>
      </div>
    </AppShell>
  );
}

