'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, KPIGrid, StatCard, LineChart, ChartContainer } from '../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { DollarSign, Users, TrendingUp, FileText } from 'lucide-react';

export default function DashboardPage() {
  const { data: session, status } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const brokerId = (session?.user as any)?.id || '';

  const { data: commissions, isLoading: commissionsLoading } = useQuery({
    queryKey: ['broker-commissions', brokerId],
    queryFn: async () => {
      const response = await apiClient.getBrokerCommissions({
        from: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
        to: new Date().toISOString().split('T')[0],
      });
      return response.data;
    },
    enabled: !!brokerId && status === 'authenticated',
  });

  const { data: clients, isLoading: clientsLoading } = useQuery({
    queryKey: ['broker-clients', brokerId],
    queryFn: async () => {
      const response = await apiClient.getBrokerClients({ limit: 1 });
      return response.data;
    },
    enabled: !!brokerId && status === 'authenticated',
  });

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  const kpiItems = [
    {
      title: 'Total Clients',
      value: clients?.total || 0,
      icon: Users,
      description: 'Active clients',
    },
    {
      title: 'Total Commission',
      value: `₽${(commissions?.totalCommission || 0).toLocaleString()}`,
      icon: DollarSign,
      description: 'Last 30 days',
    },
    {
      title: 'Total Orders',
      value: commissions?.total || 0,
      icon: FileText,
      description: 'Last 30 days',
    },
    {
      title: 'Total Volume',
      value: `₽${(commissions?.totalAmount || 0).toLocaleString()}`,
      icon: TrendingUp,
      description: 'Last 30 days',
    },
  ];

  const commissionChartData = commissions?.items?.map((item: any) => ({
    name: item.period,
    commission: item.commissionAmount,
  })) || [];

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
        title="Broker Dashboard"
        description="Overview of your broker activity"
      />

      <KPIGrid items={kpiItems} columns={4} className="mb-8" />

      {commissionsLoading && (
        <div className="bg-surface border border-border rounded-lg p-6">
          <div className="animate-pulse">Loading commissions chart...</div>
        </div>
      )}

      {!commissionsLoading && commissions && commissionChartData.length > 0 && (
        <ChartContainer title="Commissions Over Time" description="Monthly commission earnings">
          <LineChart
            data={commissionChartData}
            lines={[
              { dataKey: 'commission', name: 'Commission', color: '#3b82f6' },
            ]}
            height={300}
          />
        </ChartContainer>
      )}
    </AppShell>
  );
}

