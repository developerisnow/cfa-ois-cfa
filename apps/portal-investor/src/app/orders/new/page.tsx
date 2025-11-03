'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { z } from 'zod';
import { toast } from 'sonner';

const createOrderSchema = z.object({
  investorId: z.string().uuid(),
  issuanceId: z.string().uuid(),
  amount: z.number().positive(),
});

export default function NewOrderPage() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    investorId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', // TODO: Get from session
    issuanceId: '',
    amount: '',
  });
  const [idemKey] = useState(() => crypto.randomUUID());

  const mutation = useMutation({
    mutationFn: async (data: any) => {
      const response = await apiClient.placeOrder(
        {
          createOrderRequest: {
            investorId: data.investorId,
            issuanceId: data.issuanceId,
            amount: parseFloat(data.amount),
          },
        },
        {
          headers: {
            'Idempotency-Key': idemKey,
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const validated = createOrderSchema.parse({
        ...formData,
        amount: parseFloat(formData.amount),
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
      <h1 className="text-3xl font-bold mb-6">Place Buy Order</h1>
      
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">Issuance ID</label>
          <input
            type="text"
            value={formData.issuanceId}
            onChange={(e) => setFormData({ ...formData, issuanceId: e.target.value })}
            className="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Amount</label>
          <input
            type="number"
            step="0.01"
            value={formData.amount}
            onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
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
            {mutation.isPending ? 'Placing...' : 'Place Order'}
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

