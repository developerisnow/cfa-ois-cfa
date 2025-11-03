'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, DataTable, EmptyState, Skeleton } from '../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { Download, Eye } from 'lucide-react';
import Link from 'next/link';

interface AuditEvent {
  id: string;
  actor: string;
  actorName?: string;
  action: string;
  entity: string;
  entityId?: string;
  payload?: Record<string, any>;
  ip?: string;
  userAgent?: string;
  timestamp: string;
  result?: 'success' | 'failure' | 'pending';
}

export default function AuditPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [filters, setFilters] = useState({
    actor: '',
    action: '',
    entity: '',
    from: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    to: new Date().toISOString(),
    limit: 20,
    offset: 0,
  });

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: auditData, isLoading } = useQuery({
    queryKey: ['audit-events', filters],
    queryFn: async () => {
      const response = await apiClient.getAuditEvents({
        ...filters,
        actor: filters.actor || undefined,
        action: filters.action || undefined,
        entity: filters.entity || undefined,
      });
      return response.data;
    },
    enabled: status === 'authenticated',
  });

  const exportToCSV = () => {
    if (!auditData?.items || auditData.items.length === 0) return;

    const headers = ['Timestamp', 'Actor', 'Action', 'Entity', 'Entity ID', 'Result', 'IP'];
    const rows = auditData.items.map((item: AuditEvent) => [
      new Date(item.timestamp).toLocaleString(),
      item.actorName || item.actor,
      item.action,
      item.entity,
      item.entityId || '-',
      item.result || '-',
      item.ip || '-',
    ]);

    const csv = [headers, ...rows]
      .map((row) => row.map((cell: any) => `"${cell}"`).join(','))
      .join('\n');

    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `audit-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  };

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  const columns: ColumnDef<AuditEvent>[] = [
    {
      accessorKey: 'timestamp',
      header: 'Timestamp',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">
          {new Date(row.original.timestamp).toLocaleString()}
        </span>
      ),
    },
    {
      accessorKey: 'actorName',
      header: 'Actor',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">
          {row.original.actorName || row.original.actor}
        </span>
      ),
    },
    {
      accessorKey: 'action',
      header: 'Action',
      cell: ({ row }) => (
        <span className="text-sm font-medium text-text-primary">{row.original.action}</span>
      ),
    },
    {
      accessorKey: 'entity',
      header: 'Entity',
      cell: ({ row }) => (
        <span className="text-sm text-text-secondary">{row.original.entity}</span>
      ),
    },
    {
      accessorKey: 'entityId',
      header: 'Entity ID',
      cell: ({ row }) => (
        <span className="text-sm text-text-tertiary font-mono">
          {row.original.entityId ? row.original.entityId.slice(0, 8) + '...' : '-'}
        </span>
      ),
    },
    {
      accessorKey: 'result',
      header: 'Result',
      cell: ({ row }) => {
        const result = row.original.result;
        const colors: Record<string, string> = {
          success: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          failure: 'bg-danger-100 text-danger-700 dark:bg-danger-900 dark:text-danger-300',
          pending: 'bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-300',
        };
        return (
          <span className={`px-2 py-1 text-xs font-medium rounded ${colors[result || ''] || 'bg-gray-100 text-gray-700'}`}>
            {result || '-'}
          </span>
        );
      },
    },
    {
      id: 'actions',
      header: 'Actions',
      cell: ({ row }) => (
        <Link
          href={`/audit/${row.original.id}`}
          className="flex items-center gap-1 text-primary-600 hover:text-primary-700"
        >
          <Eye className="h-4 w-4" />
          View
        </Link>
      ),
    },
  ];

  return (
    <AppShell
      user={session.user}
      sidebar={{
        items: [
          { label: 'Dashboard', href: '/' },
          { label: 'KYC', href: '/kyc' },
          { label: 'Qualification', href: '/qualification' },
          { label: 'Audit', href: '/audit' },
          { label: 'Payouts', href: '/payouts' },
        ],
      }}
    >
      <PageHeader
        title="Audit Log"
        description="View system audit events"
        actions={
          <button
            onClick={exportToCSV}
            className="flex items-center gap-2 px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500"
            disabled={!auditData?.items?.length}
          >
            <Download className="h-4 w-4" />
            Export CSV
          </button>
        }
      />

      {/* Filters */}
      <div className="mb-6 flex flex-wrap gap-4 items-end">
        <div>
          <label htmlFor="actor" className="block text-sm font-medium text-text-primary mb-1">
            Actor ID
          </label>
          <input
            id="actor"
            type="text"
            value={filters.actor}
            onChange={(e) => setFilters({ ...filters, actor: e.target.value })}
            placeholder="Filter by actor..."
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
        </div>
        <div>
          <label htmlFor="action" className="block text-sm font-medium text-text-primary mb-1">
            Action
          </label>
          <input
            id="action"
            type="text"
            value={filters.action}
            onChange={(e) => setFilters({ ...filters, action: e.target.value })}
            placeholder="Filter by action..."
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
        </div>
        <div>
          <label htmlFor="entity" className="block text-sm font-medium text-text-primary mb-1">
            Entity
          </label>
          <input
            id="entity"
            type="text"
            value={filters.entity}
            onChange={(e) => setFilters({ ...filters, entity: e.target.value })}
            placeholder="Filter by entity..."
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
        </div>
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
      </div>

      {isLoading && (
        <div className="space-y-4">
          <Skeleton className="h-64 w-full" variant="rectangular" />
        </div>
      )}

      {!isLoading && auditData && (
        <DataTable
          columns={columns}
          data={auditData.items || []}
          searchable
          searchPlaceholder="Search audit events..."
          pageSize={filters.limit}
        />
      )}

      {!isLoading && (!auditData || auditData.items?.length === 0) && (
        <EmptyState
          title="No audit events found"
          description="Audit events will appear here"
        />
      )}
    </AppShell>
  );
}
