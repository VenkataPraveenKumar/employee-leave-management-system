# Stage 1: Build Angular application
FROM node:22-alpine AS build
WORKDIR /app

# Copy package manifests and install dependencies
COPY package*.json ./
RUN npm ci --legacy-peer-deps

# Copy source and compile AOT production bundle
COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve via Nginx
FROM nginx:alpine AS runtime
COPY --from=build /app/dist/employee-leave-management-system-ui/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
