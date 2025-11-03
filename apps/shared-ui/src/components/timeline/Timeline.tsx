'use client';

import React from 'react';
import { cn } from '../../utils/cn';

export interface TimelineItem {
  id: string;
  title: string;
  description?: string;
  timestamp: Date | string;
  icon?: React.ReactNode;
  status?: 'success' | 'warning' | 'danger' | 'info';
}

interface TimelineProps {
  items: TimelineItem[];
  className?: string;
}

export function Timeline({ items, className }: TimelineProps) {
  const getStatusColor = (status?: TimelineItem['status']) => {
    switch (status) {
      case 'success':
        return 'bg-success-500';
      case 'warning':
        return 'bg-warning-500';
      case 'danger':
        return 'bg-danger-500';
      case 'info':
        return 'bg-info-500';
      default:
        return 'bg-primary-500';
    }
  };

  const formatTimestamp = (timestamp: Date | string) => {
    const date = typeof timestamp === 'string' ? new Date(timestamp) : timestamp;
    return new Intl.DateTimeFormat('ru-RU', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(date);
  };

  return (
    <div className={cn('relative', className)} role="list">
      {items.map((item, index) => (
        <div
          key={item.id}
          className={cn(
            'relative flex gap-4 pb-8',
            index === items.length - 1 && 'pb-0'
          )}
          role="listitem"
        >
          {/* Line */}
          {index !== items.length - 1 && (
            <div
              className="absolute left-5 top-12 bottom-0 w-0.5 bg-border"
              aria-hidden="true"
            />
          )}

          {/* Icon */}
          <div className="relative flex-shrink-0">
            <div
              className={cn(
                'flex items-center justify-center w-10 h-10 rounded-full border-2 border-surface',
                getStatusColor(item.status)
              )}
              role="img"
              aria-label={item.status || 'default'}
            >
              {item.icon || (
                <div className="w-2 h-2 rounded-full bg-white" />
              )}
            </div>
          </div>

          {/* Content */}
          <div className="flex-1 min-w-0">
            <div className="flex items-start justify-between gap-4 mb-1">
              <h4 className="text-sm font-semibold text-text-primary">
                {item.title}
              </h4>
              <time
                className="text-xs text-text-tertiary whitespace-nowrap"
                dateTime={
                  typeof item.timestamp === 'string'
                    ? item.timestamp
                    : item.timestamp.toISOString()
                }
              >
                {formatTimestamp(item.timestamp)}
              </time>
            </div>
            {item.description && (
              <p className="text-sm text-text-secondary">{item.description}</p>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}

