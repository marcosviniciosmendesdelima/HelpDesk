# 1. Build Stage
FROM python:3.13-slim AS build

# Instala ferramentas de build necessárias para algumas bibliotecas Python
RUN apt-get update && apt-get install -y --no-install-recommends \
    gcc \
    python3-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 2. Final Stage
FROM python:3.13-slim AS final

# Instala dependências de runtime (útil para network debugging)
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    iputils-ping \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Transfere apenas as bibliotecas prontas do estágio anterior
COPY --from=build /usr/local/lib/python3.13/site-packages /usr/local/lib/python3.13/site-packages
COPY --from=build /usr/local/bin /usr/local/bin
COPY . .

# Comando de execução
CMD ["python", "-m", "uvicorn", "src.api.main:app", "--host", "0.0.0.0", "--port", "80"]