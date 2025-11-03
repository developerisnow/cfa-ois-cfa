'use client';

import React from 'react';
import { cn } from '../../utils/cn';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { LogOut, Menu, Moon, Sun, User } from 'lucide-react';
import { useTheme } from '../theme/ThemeProvider';

interface AppShellProps {
  children: React.ReactNode;
  user?: {
    name?: string | null;
    email?: string | null;
    image?: string | null;
  };
  onSignOut?: () => void;
  sidebar?: {
    items: Array<{
      label: string;
      href: string;
      icon?: React.ReactNode;
      badge?: string | number;
    }>;
  };
  headerActions?: React.ReactNode;
}

export function AppShell({
  children,
  user,
  onSignOut,
  sidebar,
  headerActions,
}: AppShellProps) {
  const pathname = usePathname();
  const { theme, toggleTheme } = useTheme();
  const [sidebarOpen, setSidebarOpen] = React.useState(false);

  return (
    <div className="min-h-screen bg-background flex flex-col">
      {/* Header */}
      <header
        className="sticky top-0 z-sticky border-b border-border bg-surface"
        role="banner"
      >
        <div className="flex items-center justify-between h-16 px-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-4">
            {sidebar && (
              <button
                type="button"
                onClick={() => setSidebarOpen(!sidebarOpen)}
                className="p-2 rounded-md text-text-secondary hover:text-text-primary hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
                aria-label="Toggle sidebar"
                aria-expanded={sidebarOpen}
              >
                <Menu className="h-5 w-5" />
              </button>
            )}
            <Link
              href="/"
              className="text-xl font-bold text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 rounded"
            >
              OIS
            </Link>
          </div>

          <div className="flex items-center gap-4">
            {headerActions}
            
            {/* Theme Toggle */}
            <button
              type="button"
              onClick={toggleTheme}
              className="p-2 rounded-md text-text-secondary hover:text-text-primary hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
              aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} theme`}
            >
              {theme === 'dark' ? (
                <Sun className="h-5 w-5" />
              ) : (
                <Moon className="h-5 w-5" />
              )}
            </button>

            {/* User Menu */}
            {user && (
              <div className="flex items-center gap-3">
                <div className="hidden sm:flex flex-col items-end">
                  <span className="text-sm font-medium text-text-primary">
                    {user.name || 'User'}
                  </span>
                  {user.email && (
                    <span className="text-xs text-text-secondary">{user.email}</span>
                  )}
                </div>
                <div className="h-8 w-8 rounded-full bg-primary-500 flex items-center justify-center text-white">
                  <User className="h-4 w-4" />
                </div>
                {onSignOut && (
                  <button
                    type="button"
                    onClick={onSignOut}
                    className="p-2 rounded-md text-text-secondary hover:text-text-primary hover:bg-surface-hover focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
                    aria-label="Sign out"
                  >
                    <LogOut className="h-5 w-5" />
                  </button>
                )}
              </div>
            )}
          </div>
        </div>
      </header>

      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar */}
        {sidebar && (
          <>
            {/* Mobile overlay */}
            {sidebarOpen && (
              <div
                className="fixed inset-0 z-modal bg-black/50 lg:hidden"
                onClick={() => setSidebarOpen(false)}
                aria-hidden="true"
              />
            )}

            {/* Sidebar */}
            <aside
              className={cn(
                'fixed lg:static inset-y-0 left-0 z-modal lg:z-0 w-64 bg-surface border-r border-border transform transition-transform duration-200 ease-in-out lg:translate-x-0',
                sidebarOpen ? 'translate-x-0' : '-translate-x-full'
              )}
              role="navigation"
              aria-label="Main navigation"
            >
              <nav className="h-full overflow-y-auto p-4">
                <ul className="space-y-1">
                  {sidebar.items.map((item) => {
                    const isActive = pathname === item.href || pathname?.startsWith(item.href + '/');
                    return (
                      <li key={item.href}>
                        <Link
                          href={item.href}
                          className={cn(
                            'flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors',
                            isActive
                              ? 'bg-primary-50 text-primary-700 dark:bg-primary-900/50 dark:text-primary-300'
                              : 'text-text-secondary hover:bg-surface-hover hover:text-text-primary',
                            'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
                          )}
                          onClick={() => setSidebarOpen(false)}
                        >
                          {item.icon && <span className="flex-shrink-0">{item.icon}</span>}
                          <span className="flex-1">{item.label}</span>
                          {item.badge && (
                            <span className="px-2 py-0.5 text-xs font-semibold rounded-full bg-primary-100 text-primary-700 dark:bg-primary-900 dark:text-primary-300">
                              {item.badge}
                            </span>
                          )}
                        </Link>
                      </li>
                    );
                  })}
                </ul>
              </nav>
            </aside>
          </>
        )}

        {/* Main Content */}
        <main className="flex-1 overflow-y-auto" role="main">
          <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {children}
          </div>
        </main>
      </div>

      {/* Footer */}
      <footer className="border-t border-border bg-surface-alt py-4 px-4 sm:px-6 lg:px-8" role="contentinfo">
        <div className="container mx-auto text-sm text-text-secondary">
          <p>&copy; {new Date().getFullYear()} OIS. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
}

