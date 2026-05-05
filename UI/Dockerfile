FROM node:22-alpine AS build
WORKDIR /app

COPY UI/package*.json ./
RUN npm ci

COPY UI/ ./
RUN npm run build

FROM caddy:2.11.2-alpine
COPY UI/Caddyfile /etc/caddy/Caddyfile
COPY --from=build /app/dist/eflow-ui/browser /srv
EXPOSE 80
