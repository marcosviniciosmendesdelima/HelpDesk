from fastapi import FastAPI
from . import models
from .database import engine
from .controllers import chamados_controllers 

# COMANDO MÁGICO: Cria as tabelas no Postgres se elas não existirem
models.Base.metadata.create_all(bind=engine)

app = FastAPI(title="Help Desk API", version="v1")

@app.get("/api/v1/health")
async def health_check():
    return {"status": "ok", "version": "v1"}

# Incluindo as rotas que o Vitor criou
app.include_router(chamados_controllers.router)