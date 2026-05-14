import sys
import os
from fastapi import FastAPI, Depends
from sqlalchemy.orm import Session

# --- AJUSTE DE CAMINHO PARA O DOCKER ---
# Isso força o Python a reconhecer a pasta raiz 'src' não importa onde o arquivo seja executado
BASE_DIR = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
if BASE_DIR not in sys.path:
    sys.path.append(BASE_DIR)

# Importações da Infraestrutura
from src.infrastructure.database.config import engine, Base, get_db 
from src.infrastructure.database import models

# Importações dos Casos de Uso (CQRS)
from src.application.use_cases.commands.chamado_comando import CriarChamadoCommand, handle_criar_chamado
from src.application.use_cases.queries.chamado_query import handle_listar_todos_chamados 

# Importação da Mensageria
from src.infrastructure.messaging.publisher import publicar_evento 

# Cria as tabelas no banco de dados se não existirem
try:
    Base.metadata.create_all(bind=engine)
except Exception as e:
    print(f"Aviso: Não foi possível conectar ao banco para criar tabelas: {e}")

app = FastAPI(title="Help Desk API - Etapa 8.3", version="v1")

@app.get("/api/v1/health")
async def health_check():
    return {"status": "ok", "version": "v1", "cqrs": "active"}

# --- ROTA DE LEITURA (Etapa 8.3 - Lado Query) ---
# Busca dados na tabela TicketsRead sincronizada pelo .NET
@app.get("/api/v1/tickets")
def listar_tickets(db: Session = Depends(get_db)):
    return handle_listar_todos_chamados(db)

# --- ROTA DE ESCRITA (Etapa 8.1 - Lado Command) ---
# Grava no banco principal e dispara evento para o RabbitMQ
@app.post("/api/v1/tickets", status_code=201)
async def criar_ticket(command: CriarChamadoCommand, db: Session = Depends(get_db)):
    # 1. Persistência no Banco de Escrita
    novo_id = handle_criar_chamado(command, db)
    
    # 2. Publicação do Evento para Consistência Eventual
    evento = {
        "id": str(novo_id),
        "titulo": command.titulo,
        "descricao": command.descricao,
        "prioridade": command.prioridade_valor,
        "status": "Aberto",
        "tipo_evento": "TicketCriado"
    }
    publicar_evento(evento)
    
    return {"id": novo_id}