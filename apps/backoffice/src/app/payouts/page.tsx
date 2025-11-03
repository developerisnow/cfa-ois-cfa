'use client';

import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';

export default function PayoutsPage() {
  const { data: report, isLoading } = useQuery({
    queryKey: ['payouts-report'],
    queryFn: async () => {
      const from = new Date();
      from.setDate(from.getDate() - 30);
      const to = new Date();
      const response = await apiClient.getPayoutsReport({
        from: from.toISOString().split('T')[0],
        to: to.toISOString().split('T')[0],
      });
      return response.data;
    },
  });

  const runSettlementMutation = useMutation({
    mutationFn: async (date?: string) => {
      const response = await apiClient.runSettlement({ date });
      return response.data;
    },
    onSuccess: () => {
      toast.success('Settlement started');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to run settlement');
    },
  });

  return (
    <div className="container mx-auto p-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Payouts</h1>
        <button
          onClick={() => runSettlementMutation.mutate(undefined)}
          disabled={runSettlementMutation.isPending}
          className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 disabled:opacity-50"
        >
          {runSettlementMutation.isPending ? 'Running...' : 'Run Settlement'}
        </button>
      </div>

      {isLoading && <div>Loading report...</div>}

      {report && (
        <div className="bg-white p-6 rounded-lg shadow">
          <h2 className="text-xl font-semibold mb-4">Report</h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-gray-600">Total Batches</p>
              <p className="text-2xl font-bold">{report.totalBatches}</p>
            </div>
            <div>
              <p className="text-gray-600">Total Amount</p>
              <p className="text-2xl font-bold">{report.totalAmount}</p>
            </div>
            <div>
              <p className="text-gray-600">Completed Items</p>
              <p className="text-2xl font-bold">{report.completedItems}</p>
            </div>
            <div>
              <p className="text-gray-600">Failed Items</p>
              <p className="text-2xl font-bold">{report.failedItems}</p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

