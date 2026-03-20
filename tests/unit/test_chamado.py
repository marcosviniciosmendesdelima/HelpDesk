import pytest
from src.domain.entities.chamado import Chamado
from src.domain.value_objects.prioridade import Prioridade

def test_deve_criar_chamado_com_status_aberto_por_padrao():
    """
    Teste Unitário: Garante que a lógica de negócio inicial (Status 'Aberto') 
    esteja correta no nascimento da entidade.
    """
    # 1. Arrange (Organizar): Preparamos os dados de entrada
    titulo = "Monitor Quebrado"
    descricao = "O monitor da recepção não liga"
    prioridade = Prioridade("Alta")

    # 2. Act (Agir): Executamos a unidade de código (a classe Chamado)
    chamado = Chamado(titulo=titulo, descricao=descricao, prioridade=prioridade)

    # 3. Assert (Afirmar): Verificamos se o resultado é o esperado
    assert chamado.status == "Aberto"
    assert chamado.titulo == titulo

def test_ao_resolver_chamado_o_status_deve_mudar_para_resolvido():
    """
    Teste Unitário: Valida a transição de estado da entidade de forma isolada.
    """
    # Arrange
    chamado = Chamado("Teste", "Descrição", Prioridade("Baixa"))

    # Act
    chamado.resolver()

    # Assert
    assert chamado.status == "Resolvido"