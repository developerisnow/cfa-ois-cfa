'use client';

import { useSession } from 'next-auth/react';
import { redirect } from 'next/navigation';
import { AppShell, PageHeader, KPIGrid, StatCard } from '../../../../shared-ui/src';
import { FileText, DollarSign, Users } from 'lucide-react';
import Link from 'next/link';
import { useQuery } from '@tanstack/react-query';

export default function DashboardPage() {
  const { data: session, status } = useSession();
  
  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    redirect('/auth/signin');
  }

  // Check if user has issuer role
  const roles = (session.user as any)?.roles || [];
  if (!roles.includes('issuer')) {
    return <div className="p-8">Access denied. Issuer role required.</div>;
  }

  // TODO: Fetch real data from API
  const kpiData = [
    {
      title: 'Active Issuances',
      value: '-',
      description: 'Currently active',
      icon: FileText,
    },
    {
      title: 'Total Amount',
      value: '-',
      description: 'All time',
      icon: DollarSign,
    },
    {
      title: 'Investors',
      value: '-',
      description: 'Unique investors',
      icon: Users,
      trend: {
        value: 0,
        isPositive: true,
        label: 'vs last month',
      },
    },
  ];

  return (
    <AppShell
      user={session.user}
      sidebar={{
        items: [
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Issuances', href: '/issuances' },
          { label: 'Reports', href: '/reports' },
        ],
      }}
    >
      <PageHeader
        title="Dashboard"
        description="Overview of your issuances and activity"
      />

      <KPIGrid items={kpiData} columns={3} className="mb-8" />

      <div className="space-y-4">
        <Link
          href="/issuances"
          className="inline-block bg-primary-600 text-white px-6 py-3 rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
        >
          View Issuances
        </Link>
        <Link
          href="/issuances/create"
          className="inline-block bg-success-600 text-white px-6 py-3 rounded-md hover:bg-success-700 focus:outline-none focus:ring-2 focus:ring-success-500 focus:ring-offset-2 ml-4"
        >
          Create New Issuance
        </Link>
      </div>
    </AppShell>
  );
}
