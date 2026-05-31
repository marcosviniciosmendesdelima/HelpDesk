#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

PY_API_URL="http://localhost:8000/api/v1/tickets"
DOTNET_GATEWAY_URL="http://localhost:5281/api/v1/tickets"

PAYLOAD='{
  "titulo": "Teste completo de fluxo",
  "descricao": "Criando um ticket para validar o fluxo de escrita e leitura entre APIs.",
  "prioridade_valor": "Alta"
}'

if ! command -v curl >/dev/null 2>&1; then
  echo "curl não encontrado. Instale curl para executar este script." >&2
  exit 1
fi

printf "[1/3] Enviando POST para %s\n" "$PY_API_URL"
RESPONSE=$(curl -sS -X POST "$PY_API_URL" \
  -H "Content-Type: application/json" \
  -d "$PAYLOAD")

printf "Resposta POST:\n%s\n\n" "$RESPONSE"

printf "[2/3] Aguardando o evento ser processado...\n"
sleep 3

printf "[3/3] Consultando GET em %s\n" "$DOTNET_GATEWAY_URL"
RESULT=$(curl -sS "$DOTNET_GATEWAY_URL")

echo "Resposta GET:"
if command -v python3 >/dev/null 2>&1; then
  echo "$RESULT" | python3 -m json.tool
else
  echo "$RESULT"
fi
