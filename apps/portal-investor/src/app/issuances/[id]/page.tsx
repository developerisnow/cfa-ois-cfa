'use client';

import { useQuery, useMutation } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { AppShell, PageHeader, ChartContainer, LineChart, EmptyState, Skeleton } from '../../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useParams, useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import Link from 'next/link';
import { TrendingUp, Calendar, DollarSign, FileText, ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';

export default function IssuanceDetailPage() {
  const { data: session, status } = useSession();
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;
  const [orderAmount, setOrderAmount] = useState('');

  const investorId = (session?.user as any)?.id || '';

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  const { data: issuance, isLoading } = useQuery({
    queryKey: ['market-issuance', id],
    queryFn: async () => {
      const response = await apiClient.get(`/v1/market/issuances/${id}`);
      return response.data;
    },
    enabled: !!id && status === 'authenticated' && !!session,
  });

  const orderMutation = useMutation({
    mutationFn: async (amount: number) => {
      const response = await apiClient.post(
        '/v1/orders',
        {
          investorId,
          issuanceId: id,
          amount,
        },
        {
          headers: {
            'Idempotency-Key': crypto.randomUUID(),
          },
        }
      );
      return response.data;
    },
    onSuccess: () => {
      toast.success('Order placed successfully');
      router.push('/portfolio');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to place order');
    },
  });

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  const handleBuy = () => {
    const amount = parseFloat(orderAmount);
    if (!amount || amount <= 0) {
      toast.error('Please enter a valid amount');
      return;
    }
    if (issuance && amount > issuance.availableAmount) {
      toast.error('Amount exceeds available');
      return;
    }
    orderMutation.mutate(amount);
  };

  // Generate mock yield chart data
  const yieldChartData = issuance
    ? Array.from({ length: 12 }, (_, i) => {
        const date = new Date();
        date.setMonth(date.getMonth() - (11 - i));
        return {
          name: date.toLocaleDateString('ru-RU', { month: 'short', year: 'numeric' }),
          yield: issuance.yield + (Math.random() * 2 - 1),
        };
      })
    : [];

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
      {isLoading && (
        <div className="space-y-6">
          <Skeleton className="h-8 w-64" variant="text" />
          <Skeleton className="h-64 w-full" variant="rectangular" />
        </div>
      )}

      {!isLoading && !issuance && (
        <EmptyState
          title="Issuance not found"
          description="The issuance you're looking for doesn't exist or has been removed"
          action={
            <Link
              href="/catalog"
              className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              Back to Catalog
            </Link>
          }
        />
      )}

      {!isLoading && issuance && (
        <>
          <PageHeader
            title={issuance.assetName}
            description={`Issued by ${issuance.issuerName}`}
            breadcrumbs={[
              { label: 'Catalog', href: '/catalog' },
              { label: issuance.assetName },
            ]}
            actions={
              <Link
                href="/catalog"
                className="flex items-center gap-2 px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500"
              >
                <ArrowLeft className="h-4 w-4" />
                Back
              </Link>
            }
          />

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
            {/* Main Info */}
            <div className="lg:col-span-2 space-y-6">
              {/* Key Metrics */}
              <div className="bg-surface border border-border rounded-lg p-6">
                <h2 className="text-xl font-semibold text-text-primary mb-4">
                  Key Information
                </h2>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-text-secondary mb-1">Total Amount</p>
                    <p className="text-lg font-semibold text-text-primary">
                      ₽{issuance.totalAmount.toLocaleString()}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-text-secondary mb-1">Available</p>
                    <p className="text-lg font-semibold text-text-primary">
                      ₽{issuance.availableAmount.toLocaleString()}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-text-secondary mb-1">Nominal</p>
                    <p className="text-lg font-semibold text-text-primary">
                      ₽{issuance.nominal.toLocaleString()}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-text-secondary mb-1">Annual Yield</p>
                    <p className="text-lg font-semibold text-success-600">
                      {issuance.yield}%
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-text-secondary mb-1">Issue Date</p>
                    <p className="text-lg font-semibold text-text-primary">
                      {new Date(issuance.issueDate).toLocaleDateString()}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-text-secondary mb-1">Maturity Date</p>
                    <p className="text-lg font-semibold text-text-primary">
                      {new Date(issuance.maturityDate).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              </div>

              {/* Yield Chart */}
              <ChartContainer
                title="Historical Yield"
                description="Yield performance over time"
              >
                <LineChart
                  data={yieldChartData}
                  lines={[
                    {
                      dataKey: 'yield',
                      name: 'Yield %',
                      color: '#22c55e',
                    },
                  ]}
                  height={200}
                />
              </ChartContainer>

              {/* Schedule */}
              {issuance.scheduleJson && (
                <div className="bg-surface border border-border rounded-lg p-6">
                  <h2 className="text-xl font-semibold text-text-primary mb-4">
                    Payout Schedule
                  </h2>
                  <div className="space-y-2">
                    {Object.entries(issuance.scheduleJson as Record<string, any>).map(
                      ([date, amount]: [string, any]) => (
                        <div
                          key={date}
                          className="flex items-center justify-between py-2 border-b border-border last:border-0"
                        >
                          <span className="text-text-primary">
                            {new Date(date).toLocaleDateString()}
                          </span>
                          <span className="font-semibold text-text-primary">
                            ₽{parseFloat(amount).toLocaleString()}
                          </span>
                        </div>
                      )
                    )}
                  </div>
                </div>
              )}

              {/* Documents */}
              <div className="bg-surface border border-border rounded-lg p-6">
                <h2 className="text-xl font-semibold text-text-primary mb-4">
                  Documents
                </h2>
                <p className="text-text-secondary">
                  No documents available at this time.
                </p>
              </div>
            </div>

            {/* Order Panel */}
            <div className="lg:col-span-1">
              <div className="bg-surface border border-border rounded-lg p-6 sticky top-24">
                <h2 className="text-xl font-semibold text-text-primary mb-4">
                  Place Order
                </h2>

                <div className="space-y-4">
                  <div>
                    <label
                      htmlFor="amount"
                      className="block text-sm font-medium text-text-primary mb-2"
                    >
                      Amount (₽)
                    </label>
                    <input
                      id="amount"
                      type="number"
                      step="0.01"
                      min="0"
                      max={issuance.availableAmount}
                      value={orderAmount}
                      onChange={(e) => setOrderAmount(e.target.value)}
                      className="w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary placeholder:text-text-tertiary focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                      placeholder="0.00"
                      aria-label="Order amount"
                    />
                    <p className="text-xs text-text-tertiary mt-1">
                      Available: ₽{issuance.availableAmount.toLocaleString()}
                    </p>
                  </div>

                  {orderAmount && parseFloat(orderAmount) > 0 && (
                    <div className="bg-surface-alt rounded-md p-4 space-y-2">
                      <div className="flex justify-between text-sm">
                        <span className="text-text-secondary">Amount:</span>
                        <span className="text-text-primary font-medium">
                          ₽{parseFloat(orderAmount || '0').toLocaleString()}
                        </span>
                      </div>
                      <div className="flex justify-between text-sm">
                        <span className="text-text-secondary">Estimated yield:</span>
                        <span className="text-success-600 font-medium">
                          ₽
                          {(
                            (parseFloat(orderAmount || '0') * issuance.yield) /
                            100
                          ).toLocaleString()}
                        </span>
                      </div>
                    </div>
                  )}

                  <button
                    onClick={handleBuy}
                    disabled={
                      orderMutation.isPending ||
                      !orderAmount ||
                      parseFloat(orderAmount) <= 0 ||
                      parseFloat(orderAmount) > issuance.availableAmount
                    }
                    className="w-full px-6 py-3 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
                  >
                    {orderMutation.isPending ? 'Placing...' : 'Buy Now'}
                  </button>

                  <p className="text-xs text-text-tertiary text-center">
                    By placing an order, you agree to the terms and conditions.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </AppShell>
  );
}

