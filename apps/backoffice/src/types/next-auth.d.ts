import 'next-auth';
import 'next-auth/jwt';
import { DefaultSession } from 'next-auth';

declare module 'next-auth' {
  interface Session {
    user: {
      roles?: string[];
    } & DefaultSession['user'];
    accessToken?: string;
  }
}

declare module 'next-auth/jwt' {
  interface JWT {
    roles?: string[];
    accessToken?: string;
  }
}

