from should_dsl import should
from src.domain.entities.chamado import Chamado

def test_deve_criar_chamado_com_status_aberto_por_padrao():
    # Arrange
    chamado = Chamado(titulo="Erro no sistema", descricao="O sistema travou")
   
    # Act & Assert 
    chamado.status |should| be("Aberto")

def test_deve_permitir_definir_prioridade_no_chamado():
    # Arrange
    chamado = Chamado(titulo="Urgente", descricao="Servidor fora do ar")
   
    # Act
    chamado.prioridade_valor = "Alta"
   
    # Assert 
    chamado.prioridade_valor |should| be("Alta")