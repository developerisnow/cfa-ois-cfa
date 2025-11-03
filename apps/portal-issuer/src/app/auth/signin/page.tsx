'use client';

import { signIn } from 'next-auth/react';
import { useSearchParams } from 'next/navigation';
import { Suspense } from 'react';

function SignInButton() {
  const searchParams = useSearchParams();
  const callbackUrl = searchParams.get('callbackUrl') || '/dashboard';

  return (
    <button
      onClick={() => signIn('keycloak', { callbackUrl })}
      className="w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-white bg-blue-600 hover:bg-blue-700"
    >
      Sign in with Keycloak
    </button>
  );
}

export default function SignInPage() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="max-w-md w-full space-y-8 p-8 bg-white rounded-lg shadow">
        <div>
          <h2 className="text-3xl font-bold text-center">OIS Portal - Issuer</h2>
          <p className="mt-2 text-center text-gray-600">Sign in to continue</p>
        </div>
        <Suspense fallback={<div>Loading...</div>}>
          <SignInButton />
        </Suspense>
      </div>
    </div>
  );
}

