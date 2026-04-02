from fastapi import FastAPI
# --- IMPORTAÇÕES DO BANCO DE DADOS ---
from src.infrastructure.database.config import engine, Base
from src.infrastructure.database import models

# CRIA AS TABELAS NO POSTGRES AUTOMATICAMENTE
Base.metadata.create_all(bind=engine)
# -------------------------------------

# Verifique se o import está com o "S" no final:
from src.api.controllers import chamados_controllers

app = FastAPI(title="Help Desk API", version="v1")

@app.get("/api/v1/health")
async def health_check():
    return {"status": "ok", "version": "v1"}

# Verifique se a rota também usa o nome correto:
app.include_router(chamados_controllers.router)