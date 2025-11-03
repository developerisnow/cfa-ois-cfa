import { withAuth } from 'next-auth/middleware';

export default withAuth({
  callbacks: {
    authorized: ({ token }) => {
      const roles = (token?.roles as string[]) || [];
      if (!roles.includes('admin') && !roles.includes('backoffice')) {
        return false;
      }
      return !!token;
    },
  },
});

export const config = {
  matcher: ['/kyc/:path*', '/qualification/:path*', '/payouts/:path*', '/audit/:path*'],
};

