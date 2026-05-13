from uuid import UUID, uuid4
from pydantic import BaseModel

# 1. O Command (Objeto imutável com os dados de entrada)
# Seguindo o princípio de retornar apenas o Guid do recurso criado
class CriarChamadoCommand(BaseModel):
    titulo: str
    descricao: str
    prioridade_valor: str

# 2. O Handler (Quem executa a lógica de escrita no banco principal)
# Aqui garantimos a Consistência Imediata (ACID)
def handle_criar_chamado(command: CriarChamadoCommand, db_session):
    # Simulando a criação da entidade de domínio
    novo_chamado_id = uuid4()
    
    # Lógica de persistência (Write Side)
    
    # O padrão CQRS dita: retorne apenas o ID
    return novo_chamado_id