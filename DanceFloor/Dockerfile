# Build stage
FROM node AS build
WORKDIR /Docker
COPY package.json ./
RUN npm install
COPY . ./
RUN npm run build:docker

# Deploy stage
FROM nginx
WORKDIR /Docker
RUN rm -rf /usr/share/nginx/html/*
COPY nginx.conf /etc/nginx/conf.d/
COPY --from=build /Docker/dist/DanceFloor /usr/share/nginx/html
EXPOSE 80
