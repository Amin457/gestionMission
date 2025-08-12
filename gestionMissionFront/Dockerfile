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

# Étape 2 : Serveur Node.js léger
FROM node:18-alpine

# Installer serve globalement pour servir les fichiers statiques
RUN npm install -g serve

WORKDIR /app

# Copier l'application compilée depuis l'étape build
COPY --from=build /app/dist/gestion-mission-front/browser ./dist

# Exposer le port 3000
EXPOSE 3000

# Lancer le serveur avec serve
CMD ["serve", "-s", "dist", "-l", "3000"]
