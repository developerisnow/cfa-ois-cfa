'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, EmptyState, Skeleton } from '../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import Link from 'next/link';
import { TrendingUp, Calendar, DollarSign } from 'lucide-react';

export default function CatalogPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [filters, setFilters] = useState({
    status: 'open' as 'open' | 'closed' | 'all',
    sort: '-yield' as string,
    limit: 20,
    offset: 0,
  });

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data, isLoading, error } = useQuery({
    queryKey: ['market-issuances', filters],
    queryFn: async () => {
      // TODO: Use generated SDK method
      const response = await apiClient.get(`/v1/market/issuances`, {
        params: filters,
      });
      return response.data;
    },
    enabled: status === 'authenticated' && !!session,
  });

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
          { label: 'Portfolio', href: '/portfolio' },
          { label: 'Catalog', href: '/catalog' },
          { label: 'Orders', href: '/orders/new' },
          { label: 'History', href: '/history' },
        ],
      }}
    >
      <PageHeader
        title="Market Catalog"
        description="Browse available CFA issuances"
      />

      {/* Filters */}
      <div className="mb-6 flex flex-wrap gap-4">
        <select
          value={filters.status}
          onChange={(e) =>
            setFilters({ ...filters, status: e.target.value as any, offset: 0 })
          }
          className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
        >
          <option value="open">Open</option>
          <option value="closed">Closed</option>
          <option value="all">All</option>
        </select>

        <select
          value={filters.sort}
          onChange={(e) =>
            setFilters({ ...filters, sort: e.target.value, offset: 0 })
          }
          className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
        >
          <option value="-yield">Yield: High to Low</option>
          <option value="yield">Yield: Low to High</option>
          <option value="-maturityDate">Maturity: Latest</option>
          <option value="maturityDate">Maturity: Earliest</option>
          <option value="-totalAmount">Amount: Largest</option>
          <option value="totalAmount">Amount: Smallest</option>
        </select>
      </div>

      {/* Results */}
      {isLoading && (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-32 w-full" variant="rectangular" />
          ))}
        </div>
      )}

      {error && (
        <div className="text-danger-600">Error loading issuances</div>
      )}

      {data && data.items && data.items.length === 0 && (
        <EmptyState
          title="No issuances found"
          description="Try adjusting your filters"
        />
      )}

      {data && data.items && data.items.length > 0 && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
            {data.items.map((issuance: any) => (
              <Link
                key={issuance.id}
                href={`/issuances/${issuance.id}`}
                className="bg-surface border border-border rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow focus:outline-none focus:ring-2 focus:ring-primary-500"
              >
                <div className="flex items-start justify-between mb-4">
                  <div>
                    <h3 className="text-lg font-semibold text-text-primary">
                      {issuance.assetName}
                    </h3>
                    <p className="text-sm text-text-secondary">
                      {issuance.issuerName}
                    </p>
                  </div>
                  <span
                    className={`px-2 py-1 text-xs font-medium rounded ${
                      issuance.status === 'open'
                        ? 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300'
                        : 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300'
                    }`}
                  >
                    {issuance.status}
                  </span>
                </div>

                <div className="space-y-2 mb-4">
                  <div className="flex items-center gap-2 text-sm">
                    <TrendingUp className="h-4 w-4 text-success-600" />
                    <span className="text-text-primary font-medium">
                      {issuance.yield}% yield
                    </span>
                  </div>
                  <div className="flex items-center gap-2 text-sm">
                    <DollarSign className="h-4 w-4 text-text-tertiary" />
                    <span className="text-text-secondary">
                      {issuance.availableAmount.toLocaleString()} available
                    </span>
                  </div>
                  <div className="flex items-center gap-2 text-sm">
                    <Calendar className="h-4 w-4 text-text-tertiary" />
                    <span className="text-text-secondary">
                      Matures: {new Date(issuance.maturityDate).toLocaleDateString()}
                    </span>
                  </div>
                </div>

                <div className="text-sm text-text-secondary">
                  Nominal: ₽{issuance.nominal.toLocaleString()}
                </div>
              </Link>
            ))}
          </div>

          {/* Pagination */}
          {data.total > filters.limit && (
            <div className="flex items-center justify-between">
              <div className="text-sm text-text-secondary">
                Showing {filters.offset + 1} to{' '}
                {Math.min(filters.offset + filters.limit, data.total)} of{' '}
                {data.total} results
              </div>
              <div className="flex gap-2">
                <button
                  onClick={() =>
                    setFilters({
                      ...filters,
                      offset: Math.max(0, filters.offset - filters.limit),
                    })
                  }
                  disabled={filters.offset === 0}
                  className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary disabled:opacity-50 disabled:cursor-not-allowed hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  Previous
                </button>
                <button
                  onClick={() =>
                    setFilters({
                      ...filters,
                      offset: filters.offset + filters.limit,
                    })
                  }
                  disabled={filters.offset + filters.limit >= data.total}
                  className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary disabled:opacity-50 disabled:cursor-not-allowed hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </AppShell>
  );
}

