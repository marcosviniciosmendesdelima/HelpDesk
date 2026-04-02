from fastapi import APIRouter, HTTPException
from src.domain.entities.chamado import Chamado
from src.domain.value_objects.prioridade import Prioridade

from src.api.schemas.chamado_schema import ChamadoCreate

router = APIRouter(prefix="/api/v1/chamados", tags=["Chamados"])


db_fake = []

@router.post("/", status_code=201)
async def criar_novo_chamado(dados: ChamadoCreate): 
    """
    Cria um chamado integrando o Schema (Luiz), o Controller (Victor) e o Domínio (Marcos).
    """
    try:
       
        p = Prioridade(valor=dados.prioridade_valor)
       
        novo_chamado = Chamado(
            titulo=dados.titulo,
            descricao=dados.descricao,
            prioridade=p
        )
       
        db_fake.append(novo_chamado)
        return novo_chamado
       
    except ValueError as e:
      
        raise HTTPException(status_code=422, detail=str(e))

@router.get("/")
async def listar_todos_os_chamados():
    return db_fake
