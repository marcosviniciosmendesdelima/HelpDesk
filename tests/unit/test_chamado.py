from should_dsl import should
from src.domain.entities.chamado import Chamado

def test_deve_criar_chamado_com_status_aberto_por_padrao():
    chamado = Chamado(
    titulo="Erro no sistema",
    descricao="O sistema travou",
    prioridade="Alta"
)
    assert chamado.status == "Aberto"

def test_deve_permitir_definir_prioridade_no_chamado():
    chamado = Chamado(
    titulo="Urgente",
    descricao="Servidor fora do ar",
    prioridade="Alta"
    )
   
    chamado.prioridade_valor = "Alta"
   
    assert chamado.prioridade_valor == "Alta"