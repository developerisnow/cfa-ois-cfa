import { withAuth } from 'next-auth/middleware';

export default withAuth({
  callbacks: {
    authorized: ({ token }) => {
      const roles = (token?.roles as string[]) || [];
      if (!roles.includes('investor')) {
        return false;
      }
      return !!token;
    },
  },
});

export const config = {
  matcher: ['/portfolio/:path*', '/orders/:path*', '/history/:path*'],
};

