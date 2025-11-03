import { withAuth } from 'next-auth/middleware';

export default withAuth({
  callbacks: {
    authorized: ({ token }) => {
      const roles = (token?.roles as string[]) || [];
      if (!roles.includes('broker')) {
        return false;
      }
      return !!token;
    },
  },
});

export const config = {
  matcher: ['/dashboard/:path*', '/clients/:path*', '/orders/:path*', '/feed/:path*'],
};

