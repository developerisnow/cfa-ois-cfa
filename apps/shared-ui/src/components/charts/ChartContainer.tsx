'use client';

import React from 'react';
import { cn } from '../../utils/cn';

interface ChartContainerProps {
  children: React.ReactNode;
  title?: string;
  description?: string;
  className?: string;
}

export function ChartContainer({
  children,
  title,
  description,
  className,
}: ChartContainerProps) {
  return (
    <div
      className={cn(
        'bg-surface rounded-lg border border-border p-6 shadow-sm',
        className
      )}
    >
      {title && (
        <div className="mb-4">
          <h3 className="text-lg font-semibold text-text-primary">{title}</h3>
          {description && (
            <p className="text-sm text-text-secondary mt-1">{description}</p>
          )}
        </div>
      )}
      <div className="w-full">{children}</div>
    </div>
  );
}

