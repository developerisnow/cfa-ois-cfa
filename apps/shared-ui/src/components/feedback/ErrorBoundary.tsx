'use client';

import React from 'react';

type ErrorBoundaryProps = {
  children: React.ReactNode;
  fallback?: React.ReactNode;
};

type ErrorBoundaryState = {
  hasError: boolean;
  error?: Error;
};

export class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    try {
      if (typeof window !== 'undefined') {
        // Basic error reporting hook (non-PII)
        // eslint-disable-next-line no-console
        console.error('Unhandled UI error', { message: error.message, stack: error.stack, componentStack: errorInfo.componentStack });
      }
    } catch {
      // no-op
    }
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback || (
        <div role="alert" className="p-6 border border-border rounded-md bg-surface text-text-primary">
          <h2 className="text-lg font-semibold mb-1">Something went wrong</h2>
          <p className="text-sm text-text-secondary">Please refresh the page or try again later.</p>
        </div>
      );
    }
    return this.props.children;
  }
}

