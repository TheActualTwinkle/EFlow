FROM node:22-alpine AS build
WORKDIR /app

COPY UI/package*.json ./
RUN npm ci

COPY UI/ ./
RUN npm run build

FROM nginx:1.27-alpine
COPY UI/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/eflow-ui/browser /usr/share/nginx/html
EXPOSE 80
