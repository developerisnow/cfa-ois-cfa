'use client';

import React from 'react';
import { cn } from '../../utils/cn';
import { LucideIcon } from 'lucide-react';

export interface StatCardProps {
  title: string;
  value: string | number;
  description?: string;
  icon?: LucideIcon;
  trend?: {
    value: number;
    isPositive: boolean;
    label?: string;
  };
  className?: string;
}

export function StatCard({
  title,
  value,
  description,
  icon: Icon,
  trend,
  className,
}: StatCardProps) {
  return (
    <div
      className={cn(
        'bg-surface rounded-lg border border-border p-6 shadow-sm',
        className
      )}
    >
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-text-secondary mb-1">{title}</p>
          <p className="text-3xl font-bold text-text-primary mb-2">{value}</p>
          
          {description && (
            <p className="text-sm text-text-tertiary">{description}</p>
          )}
          
          {trend && (
            <div className="flex items-center gap-1 mt-2">
              <span
                className={cn(
                  'text-sm font-medium',
                  trend.isPositive ? 'text-success-600' : 'text-danger-600'
                )}
              >
                {trend.isPositive ? '+' : ''}
                {trend.value}%
              </span>
              {trend.label && (
                <span className="text-sm text-text-tertiary">{trend.label}</span>
              )}
            </div>
          )}
        </div>
        
        {Icon && (
          <div className="p-3 rounded-lg bg-primary-50 dark:bg-primary-900/20">
            <Icon className="h-6 w-6 text-primary-600 dark:text-primary-400" />
          </div>
        )}
      </div>
    </div>
  );
}

