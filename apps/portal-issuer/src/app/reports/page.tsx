'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import type { IssuerReportRow } from '@ois/api-client';
import {
  AppShell,
  PageHeader,
  DataTable,
  BarChart,
  LineChart,
  ChartContainer,
  EmptyState,
  Skeleton,
} from '../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { Download } from 'lucide-react';
import { toast } from 'sonner';
import * as XLSX from 'xlsx';

// Using types from SDK

export default function ReportsPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<'issuances' | 'payouts'>('issuances');
  const [dateRange, setDateRange] = useState({
    from: new Date(Date.now() - 90 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    to: new Date().toISOString().split('T')[0],
  });
  const [granularity, setGranularity] = useState<'day' | 'week' | 'month' | 'year'>('month');

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const issuerId = (session?.user as any)?.issuerId || (session?.user as any)?.id || '';

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  const { data: issuancesReport, isLoading: issuancesLoading } = useQuery({
    queryKey: ['issuer-issuances-report', issuerId, dateRange],
    queryFn: async () => {
      const response = await apiClient.getIssuerIssuancesReport({
        issuerId,
        from: dateRange.from,
        to: dateRange.to,
      });
      return response.data;
    },
    enabled: activeTab === 'issuances' && !!issuerId,
  });

  const { data: payoutsReport, isLoading: payoutsLoading } = useQuery({
    queryKey: ['issuer-payouts-report', issuerId, dateRange, granularity],
    queryFn: async () => {
      const response = await apiClient.getIssuerPayoutsReport({
        issuerId,
        from: dateRange.from,
        to: dateRange.to,
        granularity,
      });
      return response.data;
    },
    enabled: activeTab === 'payouts' && !!issuerId,
  });

  const issuancesColumns: ColumnDef<IssuerReportRow>[] = [
    {
      accessorKey: 'assetCode',
      header: 'Asset Code',
      cell: ({ row }) => (
        <span className="text-sm font-medium text-text-primary">
          {row.original.assetCode}
        </span>
      ),
    },
    {
      accessorKey: 'assetName',
      header: 'Asset Name',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">{row.original.assetName}</span>
      ),
    },
    {
      accessorKey: 'totalAmount',
      header: 'Total Amount',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">
          ₽{row.original.totalAmount.toLocaleString()}
        </span>
      ),
    },
    {
      accessorKey: 'soldAmount',
      header: 'Sold Amount',
      cell: ({ row }) => (
        <span className="text-sm font-medium text-text-primary">
          ₽{row.original.soldAmount.toLocaleString()}
        </span>
      ),
    },
    {
      accessorKey: 'investorsCount',
      header: 'Investors',
      cell: ({ row }) => (
        <span className="text-sm text-text-primary">
          {row.original.investorsCount}
        </span>
      ),
    },
    {
      accessorKey: 'status',
      header: 'Status',
      cell: ({ row }) => {
        const status = row.original.status;
        const colors: Record<string, string> = {
          published: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          draft: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
          closed: 'bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-300',
          redeemed: 'bg-info-100 text-info-700 dark:bg-info-900 dark:text-info-300',
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
      accessorKey: 'publishedAt',
      header: 'Published',
      cell: ({ row }) => (
        <span className="text-xs text-text-tertiary">
          {row.original.publishedAt
            ? new Date(row.original.publishedAt).toLocaleDateString()
            : '-'}
        </span>
      ),
    },
  ];

  const exportToCSV = (data: any[], filename: string) => {
    if (!data || data.length === 0) return;

    const headers = activeTab === 'issuances'
      ? ['Asset Code', 'Asset Name', 'Total Amount', 'Sold Amount', 'Investors', 'Status', 'Published']
      : ['Period', 'Total Amount', 'Payout Count', 'Investors Count'];

    const rows = data.map((item) => {
      if (activeTab === 'issuances') {
        return [
          item.assetCode,
          item.assetName,
          item.totalAmount,
          item.soldAmount,
          item.investorsCount,
          item.status,
          item.publishedAt ? new Date(item.publishedAt).toLocaleDateString() : '-',
        ];
      } else {
        return [
          item.period,
          item.totalAmount,
          item.payoutCount,
          item.investorsCount,
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
    a.download = `${filename}-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  };

  const exportToXLSX = () => {
    const data = activeTab === 'issuances'
      ? issuancesReport?.items || []
      : payoutsReport?.items || [];

    if (data.length === 0) {
      toast.error('No data to export');
      return;
    }

    try {
      const headers = activeTab === 'issuances'
        ? ['Issuance ID', 'Asset Code', 'Asset Name', 'Total Amount', 'Sold Amount', 'Investors', 'Status', 'Issue Date', 'Maturity Date']
        : ['Period', 'Total Amount', 'Payout Count', 'Investors Count'];

      const rows = data.map((item: any) => {
        if (activeTab === 'issuances') {
          return [
            item.issuanceId,
            item.assetCode,
            item.assetName,
            item.totalAmount,
            item.soldAmount,
            item.investorsCount,
            item.status,
            new Date(item.issueDate).toLocaleDateString(),
            new Date(item.maturityDate).toLocaleDateString(),
          ];
        } else {
          return [
            item.period,
            item.totalAmount,
            item.payoutCount,
            item.investorsCount,
          ];
        }
      });

      const worksheet = XLSX.utils.aoa_to_sheet([headers, ...rows]);
      const workbook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(workbook, worksheet, activeTab === 'issuances' ? 'Issuances' : 'Payouts');
      XLSX.writeFile(workbook, `${activeTab}-report-${new Date().toISOString().split('T')[0]}.xlsx`);
      toast.success('XLSX exported successfully');
    } catch (error: any) {
      toast.error(`Export failed: ${error.message}`);
    }
  };

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
        title="Reports"
        description="Issuances and payouts analytics"
        actions={
          <div className="flex gap-2">
            <button
              onClick={() =>
                exportToCSV(
                  activeTab === 'issuances'
                    ? issuancesReport?.items || []
                    : payoutsReport?.items || [],
                  activeTab === 'issuances' ? 'issuances-report' : 'payouts-report'
                )
              }
              className="flex items-center gap-2 px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500"
              disabled={
                activeTab === 'issuances'
                  ? !issuancesReport?.items?.length
                  : !payoutsReport?.items?.length
              }
            >
              <Download className="h-4 w-4" />
              Export CSV
            </button>
            <button
              onClick={exportToXLSX}
              className="flex items-center gap-2 px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500"
              disabled={
                activeTab === 'issuances'
                  ? !issuancesReport?.items?.length
                  : !payoutsReport?.items?.length
              }
            >
              <Download className="h-4 w-4" />
              Export XLSX
            </button>
          </div>
        }
      />

      {/* Filters */}
      <div className="mb-6 flex flex-wrap gap-4 items-end">
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
        {activeTab === 'payouts' && (
          <div>
            <label
              htmlFor="granularity"
              className="block text-sm font-medium text-text-primary mb-1"
            >
              Granularity
            </label>
            <select
              id="granularity"
              value={granularity}
              onChange={(e) =>
                setGranularity(e.target.value as 'day' | 'week' | 'month' | 'year')
              }
              className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <option value="day">Day</option>
              <option value="week">Week</option>
              <option value="month">Month</option>
              <option value="year">Year</option>
            </select>
          </div>
        )}
      </div>

      {/* Tabs */}
      <div className="mb-6 border-b border-border">
        <div className="flex gap-4">
          <button
            onClick={() => setActiveTab('issuances')}
            className={`px-4 py-2 font-medium border-b-2 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 rounded-t ${
              activeTab === 'issuances'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-text-secondary hover:text-text-primary'
            }`}
          >
            Issuances
          </button>
          <button
            onClick={() => setActiveTab('payouts')}
            className={`px-4 py-2 font-medium border-b-2 transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 rounded-t ${
              activeTab === 'payouts'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-text-secondary hover:text-text-primary'
            }`}
          >
            Payouts
          </button>
        </div>
      </div>

      {/* Issuances Tab */}
      {activeTab === 'issuances' && (
        <>
          {issuancesLoading && (
            <Skeleton className="h-64 w-full" variant="rectangular" />
          )}
          {!issuancesLoading && issuancesReport && (
            <>
              {/* Summary Cards */}
              {issuancesReport.summary && (
                <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                  <div className="bg-surface border border-border rounded-lg p-4">
                    <p className="text-sm text-text-secondary mb-1">Total Issuances</p>
                    <p className="text-2xl font-bold text-text-primary">
                      {issuancesReport.summary.totalIssuances}
                    </p>
                  </div>
                  <div className="bg-surface border border-border rounded-lg p-4">
                    <p className="text-sm text-text-secondary mb-1">Total Amount</p>
                    <p className="text-2xl font-bold text-text-primary">
                      ₽{issuancesReport.summary.totalAmount.toLocaleString()}
                    </p>
                  </div>
                  <div className="bg-surface border border-border rounded-lg p-4">
                    <p className="text-sm text-text-secondary mb-1">Total Sold</p>
                    <p className="text-2xl font-bold text-text-primary">
                      ₽{issuancesReport.summary.totalSold.toLocaleString()}
                    </p>
                  </div>
                  <div className="bg-surface border border-border rounded-lg p-4">
                    <p className="text-sm text-text-secondary mb-1">Total Investors</p>
                    <p className="text-2xl font-bold text-text-primary">
                      {issuancesReport.summary.totalInvestors}
                    </p>
                  </div>
                </div>
              )}

              {/* Chart */}
              {issuancesReport.items && issuancesReport.items.length > 0 && (
                <div className="mb-6">
                  <ChartContainer title="Sold vs Total Amount">
                    <BarChart
                      data={issuancesReport.items.map((item: IssuerReportRow) => ({
                        name: item.assetCode,
                        total: item.totalAmount,
                        sold: item.soldAmount,
                      }))}
                      bars={[
                        { dataKey: 'total', name: 'Total', color: '#3b82f6' },
                        { dataKey: 'sold', name: 'Sold', color: '#22c55e' },
                      ]}
                      height={300}
                    />
                  </ChartContainer>
                </div>
              )}

              {/* Table */}
              <DataTable
                columns={issuancesColumns}
                data={issuancesReport.items || []}
                searchable
                searchPlaceholder="Search issuances..."
                pageSize={10}
              />
            </>
          )}
          {!issuancesLoading && (!issuancesReport || issuancesReport.items?.length === 0) && (
            <EmptyState
              title="No issuances found"
              description="Your issuances will appear here"
            />
          )}
        </>
      )}

      {/* Payouts Tab */}
      {activeTab === 'payouts' && (
        <>
          {payoutsLoading && (
            <Skeleton className="h-64 w-full" variant="rectangular" />
          )}
          {!payoutsLoading && payoutsReport && (
            <>
              {/* Summary */}
              {payoutsReport.summary && (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                  <div className="bg-surface border border-border rounded-lg p-4">
                    <p className="text-sm text-text-secondary mb-1">Total Amount</p>
                    <p className="text-2xl font-bold text-text-primary">
                      ₽{payoutsReport.summary.totalAmount.toLocaleString()}
                    </p>
                  </div>
                  <div className="bg-surface border border-border rounded-lg p-4">
                    <p className="text-sm text-text-secondary mb-1">Total Payouts</p>
                    <p className="text-2xl font-bold text-text-primary">
                      {payoutsReport.summary.totalPayouts}
                    </p>
                  </div>
                  <div className="bg-surface border border-border rounded-lg p-4">
                    <p className="text-sm text-text-secondary mb-1">Investors</p>
                    <p className="text-2xl font-bold text-text-primary">
                      {payoutsReport.summary.totalInvestors}
                    </p>
                  </div>
                </div>
              )}

              {/* Chart */}
              {payoutsReport.items && payoutsReport.items.length > 0 && (
                <div className="mb-6">
                  <ChartContainer
                    title={`Payouts Over Time (${granularity})`}
                    description="Payout amounts by period"
                  >
                    <LineChart
                      data={payoutsReport.items.map((item: any) => ({
                        name: item.period,
                        amount: item.totalAmount,
                        count: item.payoutCount,
                      }))}
                      lines={[
                        {
                          dataKey: 'amount',
                          name: 'Amount',
                          color: '#3b82f6',
                        },
                        {
                          dataKey: 'count',
                          name: 'Count',
                          color: '#22c55e',
                        },
                      ]}
                      height={300}
                    />
                  </ChartContainer>
                </div>
              )}

              {/* Table */}
              {payoutsReport.items && payoutsReport.items.length > 0 && (
                <DataTable
                  columns={[
                    {
                      accessorKey: 'period',
                      header: 'Period',
                      cell: ({ row }) => (
                        <span className="text-sm text-text-primary">{row.original.period}</span>
                      ),
                    },
                    {
                      accessorKey: 'totalAmount',
                      header: 'Total Amount',
                      cell: ({ row }: any) => (
                        <span className="text-sm font-medium text-text-primary">
                          ₽{row.original.totalAmount.toLocaleString()}
                        </span>
                      ),
                    },
                    {
                      accessorKey: 'payoutCount',
                      header: 'Payout Count',
                      cell: ({ row }: any) => (
                        <span className="text-sm text-text-primary">
                          {row.original.payoutCount}
                        </span>
                      ),
                    },
                    {
                      accessorKey: 'investorsCount',
                      header: 'Investors',
                      cell: ({ row }: any) => (
                        <span className="text-sm text-text-primary">
                          {row.original.investorsCount}
                        </span>
                      ),
                    },
                  ]}
                  data={payoutsReport.items}
                  searchable={false}
                  pageSize={10}
                />
              )}

              {(!payoutsReport.items || payoutsReport.items.length === 0) && (
                <EmptyState
                  title="No payouts found"
                  description="Your payout history will appear here"
                />
              )}
            </>
          )}
        </>
      )}
    </AppShell>
  );
}
