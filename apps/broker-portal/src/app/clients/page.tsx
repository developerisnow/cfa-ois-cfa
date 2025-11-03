'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, DataTable, EmptyState, Skeleton } from '../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import Link from 'next/link';
import { User, CheckCircle, XCircle, Clock } from 'lucide-react';
import type { BrokerClient } from '@ois/api-client';

export default function ClientsPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [searchQuery, setSearchQuery] = useState('');
  const [pagination, setPagination] = useState({
    limit: 20,
    offset: 0,
  });

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: clientsData, isLoading } = useQuery({
    queryKey: ['broker-clients', searchQuery, pagination],
    queryFn: async () => {
      const response = await apiClient.getBrokerClients({
        ...pagination,
        query: searchQuery || undefined,
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

  const columns: ColumnDef<BrokerClient>[] = [
    {
      accessorKey: 'name',
      header: 'Client',
      cell: ({ row }) => (
        <Link
          href={`/clients/${row.original.id}`}
          className="flex items-center gap-2 text-primary-600 hover:text-primary-700"
        >
          <User className="h-4 w-4" />
          <span className="font-medium">{row.original.name}</span>
        </Link>
      ),
    },
    {
      accessorKey: 'email',
      header: 'Email',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">{row.original.email}</span>
      ),
    },
    {
      accessorKey: 'type',
      header: 'Type',
      cell: ({ row }) => (
        <span className="text-sm text-text-secondary">
          {row.original.type === 'individual' ? 'Individual' : 'Legal Entity'}
        </span>
      ),
    },
    {
      accessorKey: 'kycStatus',
      header: 'KYC Status',
      cell: ({ row }) => {
        const status = row.original.kycStatus;
        const icons = {
          approved: <CheckCircle className="h-4 w-4 text-success-600" />,
          rejected: <XCircle className="h-4 w-4 text-danger-600" />,
          pending: <Clock className="h-4 w-4 text-warning-600" />,
        };
        const colors = {
          approved: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          rejected: 'bg-danger-100 text-danger-700 dark:bg-danger-900 dark:text-danger-300',
          pending: 'bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-300',
        };
        return (
          <div className="flex items-center gap-2">
            {icons[status]}
            <span className={`px-2 py-1 text-xs font-medium rounded ${colors[status]}`}>
              {status}
            </span>
          </div>
        );
      },
    },
    {
      accessorKey: 'qualificationStatus',
      header: 'Qualification',
      cell: ({ row }) => {
        const status = row.original.qualificationStatus;
        const colors: Record<string, string> = {
          qualified: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          unqualified: 'bg-danger-100 text-danger-700 dark:bg-danger-900 dark:text-danger-300',
          none: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
        };
        return (
          <span className={`px-2 py-1 text-xs font-medium rounded ${colors[status] || ''}`}>
            {status}
          </span>
        );
      },
    },
    {
      accessorKey: 'lastActivityAt',
      header: 'Last Activity',
      cell: ({ row }) => (
        <span className="text-sm text-text-secondary">
          {row.original.lastActivityAt
            ? new Date(row.original.lastActivityAt).toLocaleDateString()
            : 'Never'}
        </span>
      ),
    },
  ];

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
        title="Clients"
        description="Manage your broker clients"
      />

      {isLoading && (
        <div className="space-y-4">
          <Skeleton className="h-64 w-full" variant="rectangular" />
        </div>
      )}

      {!isLoading && clientsData && (
        <DataTable
          columns={columns}
          data={clientsData.items || []}
          searchable
          searchPlaceholder="Search clients by name, email, or INN..."
          pageSize={pagination.limit}
        />
      )}

      {!isLoading && (!clientsData || clientsData.items?.length === 0) && (
        <EmptyState
          title="No clients found"
          description="Your clients will appear here"
        />
      )}
    </AppShell>
  );
}

