from fastapi import APIRouter, HTTPException
from src.domain.entities.chamado import Chamado
from src.domain.value_objects.prioridade import Prioridade


router = APIRouter(prefix="/api/v1/chamados", tags=["Chamados"])


db_fake = []

@router.post("/", status_code=201) # Status 201 Created para novos recursos
async def criar_novo_chamado(titulo: str, descricao: str, prioridade_valor: str):
    """
    Cria um chamado utilizando as regras de negócio do Domínio (Marcos).
    """
    try:
        # Integrando com o Value Object de Prioridade (Criado pelo Marcos)
        p = Prioridade(valor=prioridade_valor)
       
        # Integrando com a Entidade Chamado (Criada pelo Marcos)
        novo_chamado = Chamado(titulo=titulo, descricao=descricao, prioridade=p)
       
        db_fake.append(novo_chamado)
        return novo_chamado
       
    except ValueError as e:
        # Princípio 4: Tratamento de Erro Padronizado (RFC 9457)
        raise HTTPException(status_code=422, detail=str(e))

@router.get("/") # Método GET para listagem
async def listar_todos_os_chamados():
    return db_fake