
Vinícius Mendes
3:18 PM (0 minutes ago)
to me

# Build - Prepara as bibliotecas (SDK pesado)
FROM python:3.13-slim AS build
WORKDIR /app
COPY requirements.txt .
# RUN: Instala as dependências que Felipe e o Luis usaram
RUN pip install --no-cache-dir -r requirements.txt

# Final (Runtime)
FROM python:3.13-slim AS final
WORKDIR /app
# COPY --from: Transfere apenas as bibliotecas prontas (Multi-stage Build)
COPY --from=build /usr/local/lib/python3.13/site-packages /usr/local/lib/python3.13/site-packages
COPY . .

#Define a rota para o Python encontrar a pasta 'src'
ENV PYTHONPATH=/app

# ENTRYPOINT: Comando que inicia a API ao "nascer" o container
ENTRYPOINT ["python", "src/api/main.py"]
