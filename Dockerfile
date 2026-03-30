# Build - Prepara as bibliotecas (SDK pesado)
FROM python:3.13-slim AS build
WORKDIR /app
COPY requirements.txt .
# RUN: Instala as dependências que o Felipe e o Luis usaram
RUN pip install --no-cache-dir -r requirements.txt

# Final - Imagem leve para rodar no Mac (Runtime)
FROM python:3.13-slim AS final
WORKDIR /app
# COPY --from: Transfere apenas as bibliotecas prontas (Multi-stage Build)
COPY --from=build /usr/local/lib/python3.13/site-packages /usr/local/lib/python3.13/site-packages
COPY . .
# ENTRYPOINT: Comando que inicia a API ao "nascer" o container
ENTRYPOINT ["python", "main.py"]