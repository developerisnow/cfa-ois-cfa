'use client';

import React from 'react';
import {
  PieChart as RechartsPieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { ChartContainer } from './ChartContainer';
import type { ComponentProps } from 'react';

interface PieChartProps extends Omit<ComponentProps<typeof RechartsPieChart>, 'children'> {
  data: Array<{
    name: string;
    value: number;
    color?: string;
  }>;
  title?: string;
  description?: string;
  height?: number;
}

const defaultColors = [
  '#3b82f6',
  '#22c55e',
  '#f59e0b',
  '#ef4444',
  '#8b5cf6',
  '#ec4899',
  '#06b6d4',
  '#84cc16',
];

export function PieChart({
  data,
  title,
  description,
  height = 300,
  ...props
}: PieChartProps) {
  const content = (
    <ResponsiveContainer width="100%" height={height}>
      <RechartsPieChart {...props}>
        <Pie
          data={data}
          cx="50%"
          cy="50%"
          labelLine={false}
          label={({ name, percent }) =>
            `${name}: ${(percent * 100).toFixed(0)}%`
          }
          outerRadius={80}
          fill="#8884d8"
          dataKey="value"
        >
          {data.map((entry, index) => (
            <Cell
              key={`cell-${index}`}
              fill={entry.color || defaultColors[index % defaultColors.length]}
            />
          ))}
        </Pie>
        <Tooltip
          contentStyle={{
            backgroundColor: 'var(--color-surface)',
            border: '1px solid var(--color-border)',
            borderRadius: 'var(--radius-md)',
          }}
        />
        <Legend />
      </RechartsPieChart>
    </ResponsiveContainer>
  );

  if (title || description) {
    return (
      <ChartContainer title={title} description={description}>
        {content}
      </ChartContainer>
    );
  }

  return content;
}

