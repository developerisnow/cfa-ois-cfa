'use client';

import React from 'react';
import { cn } from '../../utils/cn';

interface SkeletonProps extends React.HTMLAttributes<HTMLDivElement> {
  className?: string;
  variant?: 'text' | 'circular' | 'rectangular';
}

export function Skeleton({
  className,
  variant = 'rectangular',
  ...props
}: SkeletonProps) {
  const variants = {
    text: 'h-4 rounded',
    circular: 'rounded-full',
    rectangular: 'rounded',
  };

  return (
    <div
      className={cn(
        'animate-pulse bg-surface-alt',
        variants[variant],
        className
      )}
      aria-busy="true"
      aria-label="Loading"
      {...props}
    />
  );
}

