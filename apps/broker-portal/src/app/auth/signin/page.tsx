'use client';

import { signIn } from 'next-auth/react';
import { useSearchParams } from 'next/navigation';
import { Suspense } from 'react';

function SignInButton() {
  const searchParams = useSearchParams();
  const callbackUrl = searchParams.get('callbackUrl') || '/dashboard';

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="max-w-md w-full bg-surface border border-border rounded-lg shadow-lg p-8">
        <h1 className="text-2xl font-bold text-text-primary mb-6 text-center">
          Broker Portal
        </h1>
        <p className="text-text-secondary mb-6 text-center">
          Sign in to access broker features
        </p>
        <button
          onClick={() => signIn('keycloak', { callbackUrl })}
          className="w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          Sign in with Keycloak
        </button>
      </div>
    </div>
  );
}

export default function SignInPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <SignInButton />
    </Suspense>
  );
}

