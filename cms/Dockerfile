FROM node:20-alpine AS build

RUN apk add --no-cache vips-dev build-base python3

WORKDIR /opt/app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ENV NODE_ENV=production
RUN npm run build


FROM build AS prod

RUN npm prune --omit=dev \
    && npm cache clean --force \
    && rm -rf node_modules/.cache


FROM node:20-alpine AS runtime

RUN apk add --no-cache vips

WORKDIR /opt/app
ENV NODE_ENV=production

COPY --chown=node:node --from=prod /opt/app/node_modules ./node_modules
COPY --chown=node:node --from=build /opt/app/dist ./dist
COPY --chown=node:node --from=build /opt/app/tsconfig.json ./tsconfig.json
COPY --chown=node:node --from=build /opt/app/package.json ./package.json
COPY --chown=node:node --from=build /opt/app/favicon.png ./favicon.png
COPY --chown=node:node --from=build /opt/app/public ./public
COPY --chown=node:node --from=build /opt/app/database ./database

RUN mkdir -p public/uploads && chown node:node public/uploads

USER node
EXPOSE 1337
CMD ["npm", "run", "start"]
