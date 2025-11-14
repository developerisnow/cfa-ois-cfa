'use client';

import { useState } from 'react';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { AppShell, PageHeader, EmptyState } from '../../../../shared-ui/src';

type Item = { id: string; date: string; amount: string; status: 'planned' | 'executing' | 'done' | 'cancelled' };

export default function PayoutsSchedulePage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [issuanceId, setIssuanceId] = useState('');
  const [items, setItems] = useState<Item[]>([]);

  if (status === 'loading') return <div className="p-8">Loading...</div>;
  if (!session) return null;

  const addItem = () => {
    setItems((prev) => [
      ...prev,
      { id: crypto.randomUUID(), date: new Date().toISOString().split('T')[0], amount: '0', status: 'planned' },
    ]);
  };

  const removeItem = (id: string) => setItems((prev) => prev.filter((i) => i.id !== id));

  return (
    <AppShell
      user={session.user}
      sidebar={{
        items: [
          { label: 'Dashboard', href: '/dashboard' },
          { label: 'Issuances', href: '/issuances' },
          { label: 'Reports', href: '/reports' },
          { label: 'Payouts Schedule', href: '/payouts/schedule' },
        ],
      }}
    >
      <PageHeader
        title="Payouts Schedule"
        description="Draft payout schedule for an issuance (read-only stub)"
      />

      <div className="space-y-6">
        <div className="flex gap-4 items-end">
          <div>
            <label htmlFor="issuanceId" className="block text-sm font-medium text-text-primary mb-1">Issuance ID</label>
            <input
              id="issuanceId"
              placeholder="00000000-0000-4000-8000-000000000000"
              className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary"
              value={issuanceId}
              onChange={(e) => setIssuanceId(e.target.value)}
            />
          </div>
          <button
            onClick={addItem}
            className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover"
          >
            Add Item
          </button>
          <button
            title="Endpoints missing in spec. See docs/frontend/MVP-impl.md"
            disabled
            className="px-4 py-2 rounded-md bg-gray-200 text-gray-600 cursor-not-allowed"
          >
            Save Schedule
          </button>
        </div>

        {items.length === 0 ? (
          <EmptyState title="No schedule items" description="Add items to draft a schedule (not persisted)" />
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead>
                <tr className="text-left text-text-secondary">
                  <th className="py-2 pr-4">Date</th>
                  <th className="py-2 pr-4">Amount</th>
                  <th className="py-2 pr-4">Status</th>
                  <th className="py-2 pr-4">Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((i) => (
                  <tr key={i.id} className="border-t border-border">
                    <td className="py-2 pr-4">
                      <input type="date" value={i.date} onChange={(e) => setItems((prev) => prev.map((x) => x.id === i.id ? { ...x, date: e.target.value } : x))} className="px-2 py-1 border border-border rounded" />
                    </td>
                    <td className="py-2 pr-4">
                      <input type="number" step="0.01" value={i.amount} onChange={(e) => setItems((prev) => prev.map((x) => x.id === i.id ? { ...x, amount: e.target.value } : x))} className="px-2 py-1 border border-border rounded" />
                    </td>
                    <td className="py-2 pr-4">
                      <select value={i.status} onChange={(e) => setItems((prev) => prev.map((x) => x.id === i.id ? { ...x, status: e.target.value as Item['status'] } : x))} className="px-2 py-1 border border-border rounded">
                        <option value="planned">planned</option>
                        <option value="executing">executing</option>
                        <option value="done">done</option>
                        <option value="cancelled">cancelled</option>
                      </select>
                    </td>
                    <td className="py-2 pr-4">
                      <button onClick={() => removeItem(i.id)} className="text-danger-600 hover:underline">Remove</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </AppShell>
  );
}

