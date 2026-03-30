# Build - Prepara as bibliotecas
FROM python:3.13-slim AS build
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Final - Imagem leve para rodar no Mac (Runtime)
FROM python:3.13-slim AS final
WORKDIR /app
# Transfere apenas as bibliotecas prontas
COPY --from=build /usr/local/lib/python3.13/site-packages /usr/local/lib/python3.13/site-packages
COPY . .

# Ajuste: Roda como módulo para o Python achar a pasta 'src' automaticamente
ENTRYPOINT ["python", "-m", "src.api.main"]