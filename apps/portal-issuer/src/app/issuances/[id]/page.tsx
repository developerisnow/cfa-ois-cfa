'use client';

import { useQuery, useMutation } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { useParams, useRouter } from 'next/navigation';
import { toast } from 'sonner';

export default function IssuanceDetailPage() {
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;

  const { data: issuance, isLoading } = useQuery({
    queryKey: ['issuance', id],
    queryFn: async () => {
      const response = await apiClient.getIssuance({ id });
      return response.data;
    },
    enabled: !!id,
  });

  const publishMutation = useMutation({
    mutationFn: async () => {
      await apiClient.publishIssuance({ id });
    },
    onSuccess: () => {
      toast.success('Issuance published');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to publish');
    },
  });

  const closeMutation = useMutation({
    mutationFn: async () => {
      await apiClient.closeIssuance({ id });
    },
    onSuccess: () => {
      toast.success('Issuance closed');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to close');
    },
  });

  if (isLoading) {
    return <div className="p-8">Loading...</div>;
  }

  if (!issuance) {
    return <div className="p-8">Issuance not found</div>;
  }

  return (
    <div className="container mx-auto p-8">
      <h1 className="text-3xl font-bold mb-6">Issuance {id}</h1>
      
      <div className="bg-white p-6 rounded-lg shadow mb-6">
        <div className="space-y-2">
          <p><strong>Status:</strong> {issuance.status}</p>
          <p><strong>Total Amount:</strong> {issuance.totalAmount}</p>
          <p><strong>Nominal:</strong> {issuance.nominal}</p>
          <p><strong>Issue Date:</strong> {issuance.issueDate}</p>
          <p><strong>Maturity Date:</strong> {issuance.maturityDate}</p>
        </div>
      </div>

      <div className="space-x-4">
        {issuance.status === 'draft' && (
          <button
            onClick={() => publishMutation.mutate()}
            disabled={publishMutation.isPending}
            className="bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-700 disabled:opacity-50"
          >
            {publishMutation.isPending ? 'Publishing...' : 'Publish'}
          </button>
        )}
        {issuance.status === 'published' && (
          <button
            onClick={() => closeMutation.mutate()}
            disabled={closeMutation.isPending}
            className="bg-red-600 text-white px-6 py-3 rounded-lg hover:bg-red-700 disabled:opacity-50"
          >
            {closeMutation.isPending ? 'Closing...' : 'Close'}
          </button>
        )}
        <button
          onClick={() => router.back()}
          className="bg-gray-200 text-gray-800 px-6 py-3 rounded-lg hover:bg-gray-300"
        >
          Back
        </button>
      </div>
    </div>
  );
}

