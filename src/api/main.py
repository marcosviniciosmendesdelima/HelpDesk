from fastapi import FastAPI, Depends
# --- IMPORTAÇÃO DO BANCO DE DADOS ---
from src.infrastructure.database.config import engine, Base, get_db # Certifique-se de ter o get_db configurado para fornecer sessões do banco de dados
from src.infrastructure.database import models

# Importe o Command e o Handler criado na pasta commands
from src.api.application.use_cases.commands.chamado_comando import CriarChamadoCommand, handle_criar_chamado

# Importe o seu publicador de eventos (ajuste o caminho conforme seu projeto)
from src.infrastructure.messaging.publisher import publicar_evento 

# CRIA AS TABELAS NO POSTGRES AUTOMATICAMENTE
Base.metadata.create_all(bind=engine)

app = FastAPI(title="Help Desk API", version="v1")

@app.get("/api/v1/health")
async def health_check():
    return {"status": "ok", "version": "v1"}

# ROTA DE CRIAÇÃO (8.1 - Lado da Escrita)
@app.post("/api/v1/tickets", status_code=201)
async def criar_ticket(command: CriarChamadoCommand, db=Depends(get_db)):
    # 1. Executa a lógica de escrita (Command Side)
    # Isso salva no banco principal e retorna apenas o ID (Guid)
    novo_id = handle_criar_chamado(command, db)
    
    # 2. Prepara o EVENTO para a consistência eventual
    # Aqui mandamos os dados que o .NET vai precisar para o banco de leitura
    evento = {
        "id": str(novo_id),
        "titulo": command.titulo,
        "descricao": command.descricao,
        "prioridade": command.prioridade_valor,
        "status": "Aberto",
        "tipo_evento": "TicketCriado"
    }
    
    # 3. Publica no RabbitMQ
    publicar_evento(evento)
    
    # Retorno padrão CQRS: Apenas o ID
    return {"id": novo_id}

# Mantemos os outros controllers se necessário
from src.api.controllers import chamados_controllers
app.include_router(chamados_controllers.router)