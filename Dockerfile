# 1. Build - Prepara as bibliotecas (SDK pesado)
FROM python:3.13-slim AS build
WORKDIR /app
COPY requirements.txt .
# Instala as dependências que o Felipe e o Luis usaram
RUN pip install --no-cache-dir -r requirements.txt

# 2. Final - Imagem leve para rodar no Mac (Runtime)
FROM python:3.13-slim AS final
WORKDIR /app

# Transfere apenas as bibliotecas prontas (Multi-stage Build)
COPY --from=build /usr/local/lib/python3.13/site-packages /usr/local/lib/python3.13/site-packages
COPY . .

# AJUSTE DEFINITIVO: Roda o motor Uvicorn na porta 80 que o seu Docker Desktop espera
# Estamos apontando para a variável 'app' dentro de src/api/main.py
CMD ["python", "-m", "uvicorn", "src.api.main:app", "--host", "0.0.0.0", "--port", "80"]