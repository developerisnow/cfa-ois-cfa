import { NextAuthOptions, DefaultSession } from 'next-auth';
import KeycloakProvider from 'next-auth/providers/keycloak';

export const authOptions: NextAuthOptions = {
  providers: [
    KeycloakProvider({
      clientId: process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID || 'portal-issuer',
      clientSecret: process.env.KEYCLOAK_CLIENT_SECRET || 'secret',
      issuer: `${process.env.NEXT_PUBLIC_KEYCLOAK_URL || 'http://localhost:8080'}/realms/${process.env.NEXT_PUBLIC_KEYCLOAK_REALM || 'ois-dev'}`,
    }),
  ],
  callbacks: {
    async jwt({ token, account }) {
      if (account) {
        token.accessToken = account.access_token;
        token.roles = account.access_token ? JSON.parse(Buffer.from((account.access_token as string).split('.')[1], 'base64').toString()).realm_access?.roles : [];
      }
      return token;
    },
    async session({ session, token }) {
      if (session.user) {
        session.user.roles = token.roles as string[];
        session.accessToken = token.accessToken as string;
      }
      return session;
    },
  },
  pages: {
    signIn: '/auth/signin',
  },
};

