import 'next-auth';
import 'next-auth/jwt';

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
