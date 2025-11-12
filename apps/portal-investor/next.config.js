/** @type {import('next').NextConfig} */
const path = require('path');
const nextConfig = {
  reactStrictMode: true,
  experimental: { externalDir: true },
  webpack: (config) => {
    config.resolve.alias['@'] = path.join(__dirname, 'src');
    config.resolve.modules = [
      path.join(__dirname, 'node_modules'),
      path.join(__dirname, '../shared-ui/node_modules'),
      'node_modules'
    ];
    return config;
  },
  env: {
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000',
    NEXT_PUBLIC_KEYCLOAK_URL: process.env.NEXT_PUBLIC_KEYCLOAK_URL || 'http://localhost:8080',
    NEXT_PUBLIC_KEYCLOAK_REALM: process.env.NEXT_PUBLIC_KEYCLOAK_REALM || 'ois-dev',
    NEXT_PUBLIC_KEYCLOAK_CLIENT_ID: process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID || 'portal-investor'
  }
};

module.exports = nextConfig;
