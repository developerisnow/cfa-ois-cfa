import { withAuth } from 'next-auth/middleware';

export default withAuth({
  callbacks: {
    authorized: ({ token, req }) => {
      // Check if user has issuer role
      const roles = (token?.roles as string[]) || [];
      if (!roles.includes('issuer')) {
        return false;
      }
      return !!token;
    },
  },
});

export const config = {
  matcher: ['/dashboard/:path*', '/issuances/:path*', '/reports/:path*'],
};

