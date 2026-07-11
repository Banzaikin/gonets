FROM node:18 AS build

ARG VITE_MAIN_ENV=production
ENV VITE_MAIN_ENV=${VITE_MAIN_ENV}

ARG VITE_API_BASE_URL=
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

ARG VITE_WS_URL=
ENV VITE_WS_URL=${VITE_WS_URL}

WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

FROM nginx:alpine

COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

RUN mkdir -p /etc/nginx/certs

EXPOSE 80 443

CMD ["nginx", "-g", "daemon off;"]