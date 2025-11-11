FROM node:20-alpine AS build
WORKDIR /src
# Копируем весь монорепозиторий, чтобы были доступны shared-ui и sdk
COPY apps ./apps
COPY packages ./packages
# Build SDK dependency first
RUN cd packages/sdks/ts \
  && npm install --no-audit --no-fund --include=dev \
  && npm run build
# Install shared-ui dependencies locally so module resolution works
RUN cd apps/shared-ui \
  && npm install --no-audit --no-fund --include=dev
# Then build the app
RUN cd apps/portal-issuer \
  && npm install --no-audit --no-fund --include=dev \
  && npm run build

FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
COPY --from=build /src/apps/portal-issuer/.next ./.next
COPY --from=build /src/apps/portal-issuer/node_modules ./node_modules
COPY --from=build /src/apps/portal-issuer/public ./public
COPY --from=build /src/apps/portal-issuer/package.json ./package.json
EXPOSE 3001
CMD ["npm","run","start"]
