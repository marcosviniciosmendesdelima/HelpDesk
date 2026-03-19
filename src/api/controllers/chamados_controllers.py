from fastapi import APIRouter, HTTPException
from src.domain.entities.chamado import Chamado
from src.domain.value_objects.prioridade import Prioridade


router = APIRouter(prefix="/api/v1/chamados", tags=["Chamados"])


db_fake = []

@router.post("/", status_code=201) 
async def criar_novo_chamado(titulo: str, descricao: str, prioridade_valor: str):
    """
    Cria um chamado utilizando as regras de negócio do Domínio (Marcos).
    """
    try:
        
        p = Prioridade(valor=prioridade_valor)
       
        
        novo_chamado = Chamado(titulo=titulo, descricao=descricao, prioridade=p)
       
        db_fake.append(novo_chamado)
        return novo_chamado
       
    except ValueError as e:
        
        raise HTTPException(status_code=422, detail=str(e))

@router.get("/") # Método GET para listagem
async def listar_todos_os_chamados():
    return db_fake