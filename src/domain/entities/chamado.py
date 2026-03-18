import uuid
from dataclasses import dataclass, field
from src.domain.value_objects.prioridade import Prioridade

@dataclass
class Chamado:
    # Entidade Chamado usando o Value Object Prioridade
    titulo: str
    descricao: str
    prioridade: Prioridade  # Agora o tipo é a nossa classe validada
    id: uuid.UUID = field(default_factory=uuid.uuid4)
    status: str = "Aberto"

    def resolver(self):
        """Muda o estado para Resolvido conforme o plano"""
        self.status = "Resolvido"