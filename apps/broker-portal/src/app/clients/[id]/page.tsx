'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, EmptyState, Skeleton, StatCard } from '../../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter, useParams } from 'next/navigation';
import { useEffect } from 'react';
import Link from 'next/link';
import { ArrowLeft, CheckCircle, XCircle, Clock, User, Mail } from 'lucide-react';

export default function ClientDetailPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const params = useParams();
  const clientId = params.id as string;

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: clientsData, isLoading } = useQuery({
    queryKey: ['broker-client', clientId],
    queryFn: async () => {
      const response = await apiClient.getBrokerClients({ limit: 1000 });
      const client = response.data.items?.find((c: any) => c.id === clientId);
      return client;
    },
    enabled: !!clientId && status === 'authenticated',
  });

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  if (isLoading) {
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
        <Skeleton className="h-64 w-full" variant="rectangular" />
      </AppShell>
    );
  }

  if (!clientsData) {
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
        <EmptyState
          title="Client not found"
          description="The client you're looking for doesn't exist"
        />
      </AppShell>
    );
  }

  const kycIcons: Record<string, React.ReactNode> = {
    approved: <CheckCircle className="h-5 w-5 text-success-600" />,
    rejected: <XCircle className="h-5 w-5 text-danger-600" />,
    pending: <Clock className="h-5 w-5 text-warning-600" />,
  };
  const kycIcon = kycIcons[clientsData.kycStatus] || kycIcons.pending;

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
        title={clientsData.name}
        description="Client details and status"
        breadcrumbs={[
          { label: 'Clients', href: '/clients' },
          { label: clientsData.name },
        ]}
        actions={
          <Link
            href="/clients"
            className="flex items-center gap-2 px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover"
          >
            <ArrowLeft className="h-4 w-4" />
            Back
          </Link>
        }
      />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        <div className="bg-surface border border-border rounded-lg p-6">
          <h2 className="text-lg font-semibold text-text-primary mb-4">
            Contact Information
          </h2>
          <div className="space-y-3">
            <div className="flex items-center gap-2">
              <Mail className="h-4 w-4 text-text-tertiary" />
              <span className="text-sm text-text-secondary">Email:</span>
              <span className="text-sm text-text-primary">{clientsData.email}</span>
            </div>
            {clientsData.inn && (
              <div className="flex items-center gap-2">
                <User className="h-4 w-4 text-text-tertiary" />
                <span className="text-sm text-text-secondary">INN:</span>
                <span className="text-sm text-text-primary">{clientsData.inn}</span>
              </div>
            )}
            <div className="flex items-center gap-2">
              <User className="h-4 w-4 text-text-tertiary" />
              <span className="text-sm text-text-secondary">Type:</span>
              <span className="text-sm text-text-primary">
                {clientsData.type === 'individual' ? 'Individual' : 'Legal Entity'}
              </span>
            </div>
          </div>
        </div>

        <div className="bg-surface border border-border rounded-lg p-6">
          <h2 className="text-lg font-semibold text-text-primary mb-4">
            Status
          </h2>
          <div className="space-y-4">
            <div>
              <p className="text-sm text-text-secondary mb-2">KYC Status</p>
              <div className="flex items-center gap-2">
                {kycIcon}
                <span className="text-sm font-medium text-text-primary capitalize">
                  {clientsData.kycStatus}
                </span>
              </div>
            </div>
            <div>
              <p className="text-sm text-text-secondary mb-2">Qualification Status</p>
              <span className="text-sm font-medium text-text-primary capitalize">
                {clientsData.qualificationStatus}
              </span>
            </div>
          </div>
        </div>
      </div>

      <div className="bg-surface border border-border rounded-lg p-6">
        <h2 className="text-lg font-semibold text-text-primary mb-4">
          Activity
        </h2>
        <div className="space-y-2 text-sm">
          <div className="flex justify-between">
            <span className="text-text-secondary">Client since:</span>
            <span className="text-text-primary">
              {new Date(clientsData.createdAt).toLocaleDateString()}
            </span>
          </div>
          <div className="flex justify-between">
            <span className="text-text-secondary">Last activity:</span>
            <span className="text-text-primary">
              {clientsData.lastActivityAt
                ? new Date(clientsData.lastActivityAt).toLocaleDateString()
                : 'Never'}
            </span>
          </div>
        </div>
      </div>
    </AppShell>
  );
}

