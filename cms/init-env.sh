set -e

if [ -f .env ]; then
  echo ".env уже существует, ничего не меняю"
  exit 0
fi

rand() { openssl rand -base64 32; }

cat > .env <<EOF
HOST=0.0.0.0
PORT=1337
APP_KEYS=$(rand),$(rand),$(rand),$(rand)
API_TOKEN_SALT=$(rand)
ADMIN_JWT_SECRET=$(rand)
TRANSFER_TOKEN_SALT=$(rand)
JWT_SECRET=$(rand)
ENCRYPTION_KEY=$(rand)
DATABASE_CLIENT=postgres
DATABASE_NAME=strapi
DATABASE_USERNAME=strapi
DATABASE_PASSWORD=$(openssl rand -hex 16)
EOF

echo ".env создан"