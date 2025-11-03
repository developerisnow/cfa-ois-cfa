'use client';

import React from 'react';
import { cn } from '../../utils/cn';
import { z } from 'zod';

const orderSchema = z.object({
  issuanceId: z.string().uuid(),
  amount: z.number().positive(),
});

interface OrderFormProps {
  onSubmit: (data: { issuanceId: string; amount: number }) => void | Promise<void>;
  onCancel?: () => void;
  defaultValues?: {
    issuanceId?: string;
    amount?: number;
  };
  isLoading?: boolean;
  className?: string;
}

export function OrderForm({
  onSubmit,
  onCancel,
  defaultValues,
  isLoading = false,
  className,
}: OrderFormProps) {
  const [formData, setFormData] = React.useState({
    issuanceId: defaultValues?.issuanceId || '',
    amount: defaultValues?.amount?.toString() || '',
  });
  const [errors, setErrors] = React.useState<Record<string, string>>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});

    try {
      const validated = orderSchema.parse({
        issuanceId: formData.issuanceId,
        amount: parseFloat(formData.amount),
      });

      await onSubmit(validated);
    } catch (error) {
      if (error instanceof z.ZodError) {
        const fieldErrors: Record<string, string> = {};
        error.errors.forEach((err) => {
          if (err.path[0]) {
            fieldErrors[err.path[0].toString()] = err.message;
          }
        });
        setErrors(fieldErrors);
      }
    }
  };

  return (
    <form onSubmit={handleSubmit} className={cn('space-y-6', className)}>
      <div>
        <label
          htmlFor="issuanceId"
          className="block text-sm font-medium text-text-primary mb-2"
        >
          Issuance ID
        </label>
        <input
          id="issuanceId"
          type="text"
          value={formData.issuanceId}
          onChange={(e) =>
            setFormData({ ...formData, issuanceId: e.target.value })
          }
          className={cn(
            'w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary placeholder:text-text-tertiary focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500',
            errors.issuanceId && 'border-danger-500'
          )}
          placeholder="Enter issuance UUID"
          aria-invalid={!!errors.issuanceId}
          aria-describedby={errors.issuanceId ? 'issuanceId-error' : undefined}
        />
        {errors.issuanceId && (
          <p
            id="issuanceId-error"
            className="mt-1 text-sm text-danger-600"
            role="alert"
          >
            {errors.issuanceId}
          </p>
        )}
      </div>

      <div>
        <label
          htmlFor="amount"
          className="block text-sm font-medium text-text-primary mb-2"
        >
          Amount
        </label>
        <input
          id="amount"
          type="number"
          step="0.01"
          min="0"
          value={formData.amount}
          onChange={(e) =>
            setFormData({ ...formData, amount: e.target.value })
          }
          className={cn(
            'w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary placeholder:text-text-tertiary focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500',
            errors.amount && 'border-danger-500'
          )}
          placeholder="0.00"
          aria-invalid={!!errors.amount}
          aria-describedby={errors.amount ? 'amount-error' : undefined}
        />
        {errors.amount && (
          <p
            id="amount-error"
            className="mt-1 text-sm text-danger-600"
            role="alert"
          >
            {errors.amount}
          </p>
        )}
      </div>

      <div className="flex items-center gap-4">
        <button
          type="submit"
          disabled={isLoading}
          className="px-6 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
        >
          {isLoading ? 'Submitting...' : 'Submit Order'}
        </button>
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="px-6 py-2 border border-border bg-surface text-text-primary rounded-md hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
          >
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}

