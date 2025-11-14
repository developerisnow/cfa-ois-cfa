FROM node:20-alpine AS build
WORKDIR /src
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
RUN cd apps/portal-investor \
  && npm install --no-audit --no-fund --include=dev \
  && npm run build

FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
COPY --from=build /src/apps/portal-investor/.next ./.next
COPY --from=build /src/apps/portal-investor/node_modules ./node_modules
COPY --from=build /src/apps/portal-investor/package.json ./package.json
EXPOSE 3002
CMD ["npm","run","start"]
