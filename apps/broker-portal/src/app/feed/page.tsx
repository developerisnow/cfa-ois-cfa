'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, Timeline, EmptyState, Skeleton } from '../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import { Filter } from 'lucide-react';
import type { FeedItem } from '@ois/api-client';

export default function FeedPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [filters, setFilters] = useState({
    from: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    to: new Date().toISOString(),
    types: [] as string[],
  });

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: feedData, isLoading } = useQuery({
    queryKey: ['broker-feed', filters],
    queryFn: async () => {
      const response = await apiClient.getBrokerFeed({
        from: filters.from,
        to: filters.to,
        types: filters.types.length > 0 ? filters.types.join(',') : undefined,
      });
      return response.data;
    },
    enabled: status === 'authenticated',
  });

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  const timelineItems = feedData?.items?.map((item: FeedItem) => {
    let status: 'success' | 'warning' | 'danger' | 'info' | undefined = 'info';
    if (item.status === 'completed') status = 'success';
    else if (item.status === 'failed') status = 'danger';
    
    return {
      id: item.id,
      title: item.title || `${item.type} event`,
      description: item.description || '',
      timestamp: new Date(item.timestamp),
      status,
    };
  }) || [];

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
        title="Activity Feed"
        description="Recent broker activity"
      />

      {/* Filters */}
      <div className="mb-6 flex flex-wrap gap-4 items-end">
        <div>
          <label htmlFor="from" className="block text-sm font-medium text-text-primary mb-1">
            From
          </label>
          <input
            id="from"
            type="datetime-local"
            value={filters.from.slice(0, 16)}
            onChange={(e) =>
              setFilters({ ...filters, from: new Date(e.target.value).toISOString() })
            }
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
        </div>
        <div>
          <label htmlFor="to" className="block text-sm font-medium text-text-primary mb-1">
            To
          </label>
          <input
            id="to"
            type="datetime-local"
            value={filters.to.slice(0, 16)}
            onChange={(e) =>
              setFilters({ ...filters, to: new Date(e.target.value).toISOString() })
            }
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
        </div>
        <div>
          <label htmlFor="types" className="block text-sm font-medium text-text-primary mb-1">
            Event Types
          </label>
          <select
            id="types"
            multiple
            value={filters.types}
            onChange={(e) => {
              const selected = Array.from(e.target.selectedOptions, (option) => option.value);
              setFilters({ ...filters, types: selected });
            }}
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
            size={5}
          >
            <option value="order">Order</option>
            <option value="transfer">Transfer</option>
            <option value="payout">Payout</option>
            <option value="kyc">KYC</option>
            <option value="qualification">Qualification</option>
          </select>
        </div>
      </div>

      {isLoading && (
        <div className="space-y-4">
          <Skeleton className="h-64 w-full" variant="rectangular" />
        </div>
      )}

      {!isLoading && feedData && timelineItems.length > 0 && (
        <Timeline items={timelineItems} />
      )}

      {!isLoading && (!feedData || timelineItems.length === 0) && (
        <EmptyState
          title="No activity found"
          description="Activity feed will appear here"
        />
      )}
    </AppShell>
  );
}

