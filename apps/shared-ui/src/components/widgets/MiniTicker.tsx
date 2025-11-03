'use client';

import React from 'react';
import { cn } from '../../utils/cn';

interface MiniTickerItem {
  label: string;
  value: string | number;
  change?: {
    value: number;
    isPositive: boolean;
  };
}

interface MiniTickerProps {
  items: MiniTickerItem[];
  className?: string;
}

export function MiniTicker({ items, className }: MiniTickerProps) {
  return (
    <div
      className={cn(
        'flex items-center gap-8 overflow-x-auto py-4 bg-surface-alt border-b border-border',
        className
      )}
      role="marquee"
      aria-live="polite"
    >
      {items.map((item, index) => (
        <div
          key={index}
          className="flex items-center gap-3 flex-shrink-0"
        >
          <span className="text-sm font-medium text-text-secondary">
            {item.label}:
          </span>
          <span className="text-sm font-semibold text-text-primary">
            {item.value}
          </span>
          {item.change && (
            <span
              className={cn(
                'text-xs font-medium',
                item.change.isPositive
                  ? 'text-success-600'
                  : 'text-danger-600'
              )}
            >
              {item.change.isPositive ? '+' : ''}
              {item.change.value}%
            </span>
          )}
        </div>
      ))}
    </div>
  );
}

