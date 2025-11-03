'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { z } from 'zod';
import { toast } from 'sonner';

const createIssuanceSchema = z.object({
  assetId: z.string().uuid(),
  issuerId: z.string().uuid(),
  totalAmount: z.number().positive(),
  nominal: z.number().positive(),
  issueDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
  maturityDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
});

export default function CreateIssuancePage() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    assetId: '',
    issuerId: '',
    totalAmount: '',
    nominal: '',
    issueDate: '',
    maturityDate: '',
  });

  const mutation = useMutation({
    mutationFn: async (data: any) => {
      const response = await apiClient.createIssuance({
        createIssuanceRequest: {
          assetId: data.assetId,
          issuerId: data.issuerId,
          totalAmount: parseFloat(data.totalAmount),
          nominal: parseFloat(data.nominal),
          issueDate: data.issueDate,
          maturityDate: data.maturityDate,
        },
      });
      return response.data;
    },
    onSuccess: (data) => {
      toast.success('Issuance created successfully');
      router.push(`/issuances/${data?.id}`);
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to create issuance');
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const validated = createIssuanceSchema.parse({
        ...formData,
        totalAmount: parseFloat(formData.totalAmount),
        nominal: parseFloat(formData.nominal),
      });
      
      mutation.mutate(validated);
    } catch (error) {
      if (error instanceof z.ZodError) {
        toast.error(error.errors[0].message);
      }
    }
  };

  return (
    <div className="container mx-auto p-8 max-w-2xl">
      <h1 className="text-3xl font-bold mb-6">Create Issuance</h1>
      
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">Asset ID</label>
          <input
            type="text"
            value={formData.assetId}
            onChange={(e) => setFormData({ ...formData, assetId: e.target.value })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Issuer ID</label>
          <input
            type="text"
            value={formData.issuerId}
            onChange={(e) => setFormData({ ...formData, issuerId: e.target.value })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Total Amount</label>
          <input
            type="number"
            step="0.01"
            value={formData.totalAmount}
            onChange={(e) => setFormData({ ...formData, totalAmount: e.target.value })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Nominal</label>
          <input
            type="number"
            step="0.01"
            value={formData.nominal}
            onChange={(e) => setFormData({ ...formData, nominal: e.target.value })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Issue Date</label>
          <input
            type="date"
            value={formData.issueDate}
            onChange={(e) => setFormData({ ...formData, issueDate: e.target.value })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Maturity Date</label>
          <input
            type="date"
            value={formData.maturityDate}
            onChange={(e) => setFormData({ ...formData, maturityDate: e.target.value })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div className="flex space-x-4">
          <button
            type="submit"
            disabled={mutation.isPending}
            className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 disabled:opacity-50"
          >
            {mutation.isPending ? 'Creating...' : 'Create'}
          </button>
          <button
            type="button"
            onClick={() => router.back()}
            className="bg-gray-200 text-gray-800 px-6 py-3 rounded-lg hover:bg-gray-300"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}

