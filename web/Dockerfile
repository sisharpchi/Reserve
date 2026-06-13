# Stage 1: Build the Angular SPA
FROM node:20-alpine AS build
WORKDIR /app

# Copy dependency configs
COPY package*.json ./

# Install packages
RUN npm ci

# Copy codebase
COPY tsconfig*.json angular.json ./
COPY src ./src
COPY public ./public

# Run build
RUN npm run build -- --configuration=production

# Stage 2: Serve using Nginx
FROM nginx:stable-alpine
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
