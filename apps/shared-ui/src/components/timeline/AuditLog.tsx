'use client';

import React from 'react';
import { Timeline, type TimelineItem } from './Timeline';
import { Activity, User, Shield, FileText } from 'lucide-react';

export interface AuditLogEntry {
  id: string;
  actor: string;
  action: string;
  entity: string;
  entityId: string;
  timestamp: Date | string;
  ip?: string;
  userAgent?: string;
  metadata?: Record<string, unknown>;
}

interface AuditLogProps {
  entries: AuditLogEntry[];
  className?: string;
}

const getActionIcon = (action: string) => {
  if (action.includes('login') || action.includes('auth')) {
    return <Shield className="h-4 w-4" />;
  }
  if (action.includes('create') || action.includes('update') || action.includes('delete')) {
    return <FileText className="h-4 w-4" />;
  }
  if (action.includes('view') || action.includes('read')) {
    return <Activity className="h-4 w-4" />;
  }
  return <User className="h-4 w-4" />;
};

const getActionStatus = (action: string): TimelineItem['status'] => {
  if (action.includes('delete') || action.includes('reject') || action.includes('fail')) {
    return 'danger';
  }
  if (action.includes('create') || action.includes('approve') || action.includes('success')) {
    return 'success';
  }
  if (action.includes('update') || action.includes('modify')) {
    return 'info';
  }
  return undefined;
};

export function AuditLog({ entries, className }: AuditLogProps) {
  const timelineItems: TimelineItem[] = entries.map((entry) => ({
    id: entry.id,
    title: `${entry.action} ${entry.entity}`,
    description: `Actor: ${entry.actor}${entry.ip ? ` | IP: ${entry.ip}` : ''}`,
    timestamp: entry.timestamp,
    icon: getActionIcon(entry.action),
    status: getActionStatus(entry.action),
  }));

  return <Timeline items={timelineItems} className={className} />;
}
