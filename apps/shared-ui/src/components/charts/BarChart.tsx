'use client';

import React from 'react';
import {
  BarChart as RechartsBarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { ChartContainer } from './ChartContainer';
import type { ComponentProps } from 'react';

interface BarChartProps extends Omit<ComponentProps<typeof RechartsBarChart>, 'children'> {
  data: Array<Record<string, unknown>>;
  bars: Array<{
    dataKey: string;
    name: string;
    color?: string;
  }>;
  title?: string;
  description?: string;
  height?: number;
}

const chartColors = [
  '#3b82f6',
  '#22c55e',
  '#f59e0b',
  '#ef4444',
  '#8b5cf6',
  '#ec4899',
  '#06b6d4',
  '#84cc16',
];

export function BarChart({
  data,
  bars,
  title,
  description,
  height = 300,
  ...props
}: BarChartProps) {
  const content = (
    <ResponsiveContainer width="100%" height={height}>
      <RechartsBarChart data={data} {...props}>
        <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
        <XAxis
          dataKey="name"
          stroke="var(--color-text-secondary)"
          fontSize={12}
        />
        <YAxis stroke="var(--color-text-secondary)" fontSize={12} />
        <Tooltip
          contentStyle={{
            backgroundColor: 'var(--color-surface)',
            border: '1px solid var(--color-border)',
            borderRadius: 'var(--radius-md)',
          }}
        />
        <Legend />
        {bars.map((bar, index) => (
          <Bar
            key={bar.dataKey}
            dataKey={bar.dataKey}
            name={bar.name}
            fill={bar.color || chartColors[index % chartColors.length]}
          />
        ))}
      </RechartsBarChart>
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

