'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { SessionProvider } from 'next-auth/react';
import { ThemeProvider } from '../../../shared-ui/src';
import { useState } from 'react';

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000,
            refetchOnWindowFocus: false,
          },
        },
      })
  );

  return (
    <ThemeProvider defaultTheme="light">
      <SessionProvider>
        <QueryClientProvider client={queryClient}>
          {children}
        </QueryClientProvider>
      </SessionProvider>
    </ThemeProvider>
  );
}

