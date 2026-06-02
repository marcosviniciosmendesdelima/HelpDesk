#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

echo "[1/6] Buildando imagem do gateway (no-cache)"
docker-compose build --no-cache gateway

echo "[2/6] Recriando container do gateway"
docker-compose up -d --force-recreate gateway

echo "[3/6] Aguardando gateway iniciar e conectar ao RabbitMQ (timeout 60s)"
timeout=60
interval=2
elapsed=0
until docker logs helpdesk-gateway 2>/dev/null | grep -qE "Consumer iniciado na fila|RabbitMQ conectado|Now listening on"; do
  sleep $interval
  elapsed=$((elapsed + interval))
  if [ "$elapsed" -ge "$timeout" ]; then
    echo "Timeout esperando gateway ficar pronto. Saindo com erro." >&2
    docker logs --tail 200 helpdesk-gateway || true
    exit 1
  fi
done

echo "[4/6] Publicando evento de teste (python3 teste_rabbit.py)"
python3 teste_rabbit.py || true

echo "[5/6] Mostrando logs do gateway (ultimas 200 linhas)"
docker logs --tail 200 helpdesk-gateway || true

echo "[6/6] Verificando tabela TicketsRead (ultimas 5 registros)"
docker exec helpdesk-db-helpdesk-1 psql -U postgres -d postgres -c 'SELECT * FROM "TicketsRead" ORDER BY datacriacao DESC LIMIT 5;'

echo "Script finalizado."
