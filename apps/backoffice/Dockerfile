FROM node:20-alpine AS build
WORKDIR /src
COPY apps ./apps
COPY packages ./packages
RUN cd apps/backoffice \
  && npm install --no-audit --no-fund --include=dev \
  && npm run build

FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
COPY --from=build /src/apps/backoffice/.next ./.next
COPY --from=build /src/apps/backoffice/node_modules ./node_modules
COPY --from=build /src/apps/backoffice/package.json ./package.json
EXPOSE 3003
CMD ["npm","run","start"]
