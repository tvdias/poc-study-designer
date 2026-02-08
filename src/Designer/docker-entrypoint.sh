#!/bin/sh
set -e

# Default API URL if not provided
API_URL=${API_URL:-http://api:8080}

# Replace placeholder in nginx config with actual API URL
envsubst '${API_URL}' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf

# Start nginx
exec nginx -g 'daemon off;'
