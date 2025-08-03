# Étape 1 : Build Angular
FROM node:18-alpine AS build

WORKDIR /app

# Copier package.json et package-lock.json
COPY package*.json ./

# Installer les dépendances
RUN npm ci

# Copier tout le code source
COPY . .

# Construire l'application Angular en mode développement (sans SSR)
RUN npx ng build --configuration=development --ssr=false

# Étape 2 : image NGINX standard
FROM nginx

# Copier l'application compilée depuis l'étape build
COPY --from=build /app/dist/gestion-mission-front/browser /usr/share/nginx/html

# Copier la configuration nginx
COPY nginx.conf /etc/nginx/nginx.conf

# Exposer le port 80
EXPOSE 80

# Lancer nginx en premier plan
CMD ["nginx", "-g", "daemon off;"]
