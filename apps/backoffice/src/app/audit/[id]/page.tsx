'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, EmptyState, Skeleton } from '../../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter, useParams } from 'next/navigation';
import { useEffect } from 'react';
import Link from 'next/link';
import { ArrowLeft } from 'lucide-react';

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

export default function AuditEventDetailPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const params = useParams();
  const eventId = params.id as string;

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: event, isLoading } = useQuery({
    queryKey: ['audit-event', eventId],
    queryFn: async () => {
      const response = await apiClient.getAuditEvent(eventId);
      return response.data;
    },
    enabled: !!eventId && status === 'authenticated',
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
            { label: 'Dashboard', href: '/' },
            { label: 'KYC', href: '/kyc' },
            { label: 'Qualification', href: '/qualification' },
            { label: 'Audit', href: '/audit' },
            { label: 'Payouts', href: '/payouts' },
          ],
        }}
      >
        <Skeleton className="h-64 w-full" variant="rectangular" />
      </AppShell>
    );
  }

  if (!event) {
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
        <EmptyState
          title="Audit event not found"
          description="The audit event you're looking for doesn't exist"
        />
      </AppShell>
    );
  }

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
        title={`Audit Event ${eventId.slice(0, 8)}`}
        description="Audit event details"
        breadcrumbs={[
          { label: 'Audit', href: '/audit' },
          { label: eventId.slice(0, 8) },
        ]}
        actions={
          <Link
            href="/audit"
            className="flex items-center gap-2 px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover"
          >
            <ArrowLeft className="h-4 w-4" />
            Back
          </Link>
        }
      />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-surface border border-border rounded-lg p-6">
          <h2 className="text-lg font-semibold text-text-primary mb-4">Event Information</h2>
          <div className="space-y-3 text-sm">
            <div className="flex justify-between">
              <span className="text-text-secondary">ID:</span>
              <span className="text-text-primary font-mono">{event.id}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-text-secondary">Timestamp:</span>
              <span className="text-text-primary">
                {new Date(event.timestamp).toLocaleString()}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-text-secondary">Actor:</span>
              <span className="text-text-primary">{event.actorName || event.actor}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-text-secondary">Action:</span>
              <span className="text-text-primary font-medium">{event.action}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-text-secondary">Entity:</span>
              <span className="text-text-primary">{event.entity}</span>
            </div>
            {event.entityId && (
              <div className="flex justify-between">
                <span className="text-text-secondary">Entity ID:</span>
                <span className="text-text-primary font-mono">{event.entityId}</span>
              </div>
            )}
            {event.result && (
              <div className="flex justify-between">
                <span className="text-text-secondary">Result:</span>
                <span className="text-text-primary">{event.result}</span>
              </div>
            )}
          </div>
        </div>

        <div className="bg-surface border border-border rounded-lg p-6">
          <h2 className="text-lg font-semibold text-text-primary mb-4">Request Details</h2>
          <div className="space-y-3 text-sm">
            {event.ip && (
              <div className="flex justify-between">
                <span className="text-text-secondary">IP Address:</span>
                <span className="text-text-primary font-mono">{event.ip}</span>
              </div>
            )}
            {event.userAgent && (
              <div className="flex flex-col">
                <span className="text-text-secondary mb-1">User Agent:</span>
                <span className="text-text-primary text-xs break-all">{event.userAgent}</span>
              </div>
            )}
          </div>
        </div>
      </div>

      {event.payload && Object.keys(event.payload).length > 0 && (
        <div className="mt-6 bg-surface border border-border rounded-lg p-6">
          <h2 className="text-lg font-semibold text-text-primary mb-4">Payload</h2>
          <pre className="bg-background p-4 rounded-md overflow-auto text-sm text-text-primary font-mono">
            {JSON.stringify(event.payload, null, 2)}
          </pre>
        </div>
      )}
    </AppShell>
  );
}

