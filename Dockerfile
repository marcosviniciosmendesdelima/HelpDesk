# --- Estágio 1: Build (Ambiente de Construção) ---
FROM python:3.13-slim AS build

# Instala compiladores apenas no build
RUN apt-get update && apt-get install -y --no-install-recommends \
    gcc \
    python3-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY requirements.txt .

# Instala dependências em um diretório local para facilitar a cópia
RUN pip install --no-cache-dir --user -r requirements.txt

# --- Estágio 2: Final (Ambiente de Execução) ---
FROM python:3.13-slim AS final

WORKDIR /app

# Copia apenas as dependências instaladas (mais leve e seguro)
COPY --from=build /root/.local /root/.local
COPY . .

# Atualiza PATH para encontrar os binários (uvicorn, etc)
ENV PATH=/root/.local/bin:$PATH

# Segurança: Cria e usa um usuário sem privilégios de root
RUN useradd -m appuser
USER appuser

# Execução
CMD ["python", "-m", "uvicorn", "src.api.main:app", "--host", "0.0.0.0", "--port", "80"]