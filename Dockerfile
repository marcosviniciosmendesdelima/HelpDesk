# 1. Build - Prepara as bibliotecas
FROM python:3.13-slim AS build
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 2. Final - Imagem leve para rodar
FROM python:3.13-slim AS final
WORKDIR /app

# Define que a raiz do projeto é a pasta /app (evita erro de ModuleNotFound)
ENV PYTHONPATH=/app

# Copia as bibliotecas instaladas na fase de build
COPY --from=build /usr/local/lib/python3.13/site-packages /usr/local/lib/python3.13/site-packages
COPY . .

# AJUSTE DEFINITIVO: Apenas UMA linha CMD. 
# Usamos o caminho direto para o uvicorn que é mais estável no Windows/Docker
CMD ["uvicorn", "src.api.main:app", "--host", "0.0.0.0", "--port", "8000"]