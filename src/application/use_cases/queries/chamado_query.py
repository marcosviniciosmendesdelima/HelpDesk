from sqlalchemy import text
from sqlalchemy.orm import Session

def handle_listar_todos_chamados(db: Session):
    """
    CQRS - Lado de Leitura (Query Side)
    Busca os dados na tabela TicketsRead que foi sincronizada pelo Gateway .NET
    """
    try:
        # Usamos mappings().all() para o SQLAlchemy retornar uma lista de dicionários
        # Isso garante que o FastAPI consiga converter para JSON automaticamente
        query = text("SELECT id, titulo, status, prioridade FROM TicketsRead")
        result = db.execute(query)
        
        return result.mappings().all()
        
    except Exception as e:
        # Caso a tabela ainda não exista (antes do primeiro ticket ser criado)
        # retornamos uma lista vazia em vez de quebrar a API
        print(f"Erro ao consultar tabela de leitura: {e}")
        return []