'use client';

import React from 'react';
import {
  LineChart as RechartsLineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { ChartContainer } from './ChartContainer';
import type { ComponentProps } from 'react';

interface LineChartProps extends Omit<ComponentProps<typeof RechartsLineChart>, 'children'> {
  data: Array<Record<string, unknown>>;
  lines: Array<{
    dataKey: string;
    name: string;
    color?: string;
    strokeWidth?: number;
  }>;
  title?: string;
  description?: string;
  height?: number;
}

const chartColors = [
  '#3b82f6', // primary
  '#22c55e', // success
  '#f59e0b', // warning
  '#ef4444', // danger
  '#8b5cf6', // purple
  '#ec4899', // pink
  '#06b6d4', // cyan
  '#84cc16', // lime
];

export function LineChart({
  data,
  lines,
  title,
  description,
  height = 300,
  ...props
}: LineChartProps) {
  const content = (
    <ResponsiveContainer width="100%" height={height}>
      <RechartsLineChart data={data} {...props}>
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
        {lines.map((line, index) => (
          <Line
            key={line.dataKey}
            type="monotone"
            dataKey={line.dataKey}
            name={line.name}
            stroke={line.color || chartColors[index % chartColors.length]}
            strokeWidth={line.strokeWidth || 2}
          />
        ))}
      </RechartsLineChart>
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

