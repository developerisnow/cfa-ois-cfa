'use client';

import React from 'react';
import { StatCard, StatCardProps } from './StatCard';
import { cn } from '../../utils/cn';

interface KPIGridProps {
  items: StatCardProps[];
  columns?: 1 | 2 | 3 | 4;
  className?: string;
}

export function KPIGrid({
  items,
  columns = 3,
  className,
}: KPIGridProps) {
  const gridCols = {
    1: 'grid-cols-1',
    2: 'grid-cols-1 sm:grid-cols-2',
    3: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3',
    4: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-4',
  };

  return (
    <div
      className={cn(
        'grid gap-6',
        gridCols[columns],
        className
      )}
      role="region"
      aria-label="Key Performance Indicators"
    >
      {items.map((item, index) => (
        <StatCard key={index} {...item} />
      ))}
    </div>
  );
}

