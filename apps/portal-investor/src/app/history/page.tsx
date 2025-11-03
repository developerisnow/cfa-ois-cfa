'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, DataTable, EmptyState, Skeleton } from '../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { Download } from 'lucide-react';

interface TxHistoryItem {
  id: string;
  type: 'transfer' | 'redeem' | 'issue';
  issuanceId: string;
  issuanceCode: string;
  amount: number;
  status: 'pending' | 'confirmed' | 'failed';
  dltTxHash: string | null;
  createdAt: string;
  confirmedAt: string | null;
}

interface PayoutItem {
  id: string;
  batchId: string;
  issuanceId: string;
  investorId: string;
  amount: number;
  status: 'pending' | 'executed' | 'failed';
  executedAt: string | null;
}

export default function HistoryPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<'transactions' | 'payouts'>('transactions');
  const [dateRange, setDateRange] = useState({
    from: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    to: new Date().toISOString().split('T')[0],
  });

  const investorId = (session?.user as any)?.id || '';

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: transactions, isLoading: txLoading } = useQuery({
    queryKey: ['investor-transactions', investorId, dateRange],
    queryFn: async () => {
      const response = await apiClient.get(`/v1/investors/${investorId}/transactions`, {
        params: {
          from: dateRange.from,
          to: dateRange.to,
          type: 'all',
        },
      });
      return response.data;
    },
    enabled: activeTab === 'transactions' && !!investorId && status === 'authenticated',
  });

  const { data: payouts, isLoading: payoutsLoading } = useQuery({
    queryKey: ['investor-payouts', investorId, dateRange],
    queryFn: async () => {
      const response = await apiClient.get(`/v1/investors/${investorId}/payouts`, {
        params: {
          from: dateRange.from,
          to: dateRange.to,
        },
      });
      return response.data;
    },
    enabled: activeTab === 'payouts' && !!investorId && status === 'authenticated',
  });

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  const txColumns: ColumnDef<TxHistoryItem>[] = [
    {
      accessorKey: 'createdAt',
      header: 'Date',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">
          {new Date(row.original.createdAt).toLocaleDateString('ru-RU', {
            dateStyle: 'medium',
            timeStyle: 'short',
          })}
        </span>
      ),
    },
    {
      accessorKey: 'type',
      header: 'Type',
      cell: ({ row }) => {
        const type = row.original.type;
        const colors: Record<string, string> = {
          issue: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          transfer: 'bg-info-100 text-info-700 dark:bg-info-900 dark:text-info-300',
          redeem: 'bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-300',
        };
        return (
          <span
            className={`px-2 py-1 text-xs font-medium rounded ${colors[type] || ''}`}
          >
            {type}
          </span>
        );
      },
    },
    {
      accessorKey: 'issuanceCode',
      header: 'Issuance',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">{row.original.issuanceCode}</span>
      ),
    },
    {
      accessorKey: 'amount',
      header: 'Amount',
      cell: ({ row }) => (
        <span className="text-sm font-medium text-text-primary">
          ₽{row.original.amount.toLocaleString()}
        </span>
      ),
    },
    {
      accessorKey: 'status',
      header: 'Status',
      cell: ({ row }) => {
        const status = row.original.status;
        const colors: Record<string, string> = {
          confirmed: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          pending: 'bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-300',
          failed: 'bg-danger-100 text-danger-700 dark:bg-danger-900 dark:text-danger-300',
        };
        return (
          <span
            className={`px-2 py-1 text-xs font-medium rounded ${colors[status] || ''}`}
          >
            {status}
          </span>
        );
      },
    },
    {
      accessorKey: 'dltTxHash',
      header: 'DLT Hash',
      cell: ({ row }) => (
        <span className="text-xs text-text-tertiary font-mono">
          {row.original.dltTxHash
            ? `${row.original.dltTxHash.substring(0, 8)}...`
            : '-'}
        </span>
      ),
    },
  ];

  const payoutColumns: ColumnDef<PayoutItem>[] = [
    {
      accessorKey: 'executedAt',
      header: 'Date',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">
          {row.original.executedAt
            ? new Date(row.original.executedAt).toLocaleDateString('ru-RU', {
                dateStyle: 'medium',
                timeStyle: 'short',
              })
            : '-'}
        </span>
      ),
    },
    {
      accessorKey: 'issuanceId',
      header: 'Issuance ID',
      cell: ({ row }) => (
        <span className="text-xs text-text-tertiary font-mono">
          {row.original.issuanceId.substring(0, 8)}...
        </span>
      ),
    },
    {
      accessorKey: 'amount',
      header: 'Amount',
      cell: ({ row }) => (
        <span className="text-sm font-medium text-text-primary">
          ₽{row.original.amount.toLocaleString()}
        </span>
      ),
    },
    {
      accessorKey: 'status',
      header: 'Status',
      cell: ({ row }) => {
        const status = row.original.status;
        const colors: Record<string, string> = {
          executed: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          pending: 'bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-300',
          failed: 'bg-danger-100 text-danger-700 dark:bg-danger-900 dark:text-danger-300',
        };
        return (
          <span
            className={`px-2 py-1 text-xs font-medium rounded ${colors[status] || ''}`}
          >
            {status}
          </span>
        );
      },
    },
  ];

  const exportToCSV = (data: any[], filename: string) => {
    if (!data || data.length === 0) return;

    const headers = activeTab === 'transactions' 
      ? ['Date', 'Type', 'Issuance', 'Amount', 'Status', 'DLT Hash']
      : ['Date', 'Issuance ID', 'Amount', 'Status'];

    const rows = data.map((item) => {
      if (activeTab === 'transactions') {
        return [
          new Date(item.createdAt).toLocaleDateString(),
          item.type,
          item.issuanceCode,
          item.amount,
          item.status,
          item.dltTxHash || '-',
        ];
      } else {
        return [
          item.executedAt ? new Date(item.executedAt).toLocaleDateString() : '-',
          item.issuanceId,
          item.amount,
          item.status,
        ];
      }
    });

    const csv = [headers, ...rows]
      .map((row) => row.map((cell) => `"${cell}"`).join(','))
      .join('\n');

    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${filename}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  };

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
        title="Transaction History"
        description="View your transactions and payouts"
        actions={
          <button
            onClick={() =>
              exportToCSV(
                activeTab === 'transactions'
                  ? transactions?.items || []
                  : payouts?.items || [],
                activeTab === 'transactions' ? 'transactions' : 'payouts'
              )
            }
            className="flex items-center gap-2 px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500"
            disabled={
              activeTab === 'transactions'
                ? !transactions?.items?.length
                : !payouts?.items?.length
            }
          >
            <Download className="h-4 w-4" />
            Export CSV
          </button>
        }
      />

      {/* Date Range Filter */}
      <div className="mb-6 flex gap-4 items-end">
        <div>
          <label
            htmlFor="from"
            className="block text-sm font-medium text-text-primary mb-1"
          >
            From
          </label>
          <input
            id="from"
            type="date"
            value={dateRange.from}
            onChange={(e) =>
              setDateRange({ ...dateRange, from: e.target.value })
            }
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
        </div>
        <div>
          <label
            htmlFor="to"
            className="block text-sm font-medium text-text-primary mb-1"
          >
            To
          </label>
          <input
            id="to"
            type="date"
            value={dateRange.to}
            onChange={(e) =>
              setDateRange({ ...dateRange, to: e.target.value })
            }
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
        </div>
      </div>

      {/* Tabs */}
      <div className="mb-6 border-b border-border">
        <div className="flex gap-4">
          <button
            onClick={() => setActiveTab('transactions')}
            className={`px-4 py-2 font-medium border-b-2 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 rounded-t ${
              activeTab === 'transactions'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-text-secondary hover:text-text-primary'
            }`}
          >
            Transactions ({transactions?.total || 0})
          </button>
          <button
            onClick={() => setActiveTab('payouts')}
            className={`px-4 py-2 font-medium border-b-2 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 rounded-t ${
              activeTab === 'payouts'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-text-secondary hover:text-text-primary'
            }`}
          >
            Payouts ({payouts?.total || 0})
          </button>
        </div>
      </div>

      {/* Content */}
      {activeTab === 'transactions' && (
        <>
          {txLoading && <Skeleton className="h-64 w-full" variant="rectangular" />}
          {!txLoading && transactions && (
            <DataTable
              columns={txColumns}
              data={transactions.items || []}
              searchable
              searchPlaceholder="Search transactions..."
              pageSize={10}
            />
          )}
          {!txLoading && (!transactions || transactions.items?.length === 0) && (
            <EmptyState
              title="No transactions found"
              description="Your transaction history will appear here"
            />
          )}
        </>
      )}

      {activeTab === 'payouts' && (
        <>
          {payoutsLoading && (
            <Skeleton className="h-64 w-full" variant="rectangular" />
          )}
          {!payoutsLoading && payouts && (
            <>
              {payouts.totalAmount && (
                <div className="mb-4 p-4 bg-surface-alt rounded-lg">
                  <p className="text-sm text-text-secondary">
                    Total Payouts: ₽{payouts.totalAmount.toLocaleString()}
                  </p>
                </div>
              )}
              <DataTable
                columns={payoutColumns}
                data={payouts.items || []}
                searchable
                searchPlaceholder="Search payouts..."
                pageSize={10}
              />
            </>
          )}
          {!payoutsLoading && (!payouts || payouts.items?.length === 0) && (
            <EmptyState
              title="No payouts found"
              description="Your payout history will appear here"
            />
          )}
        </>
      )}
    </AppShell>
  );
}
