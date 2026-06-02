import pytest
from should_dsl import should

from src.domain.entities.chamado import Chamado
from src.domain.value_objects.prioridade import Prioridade


def test_f_deve_criar_chamado_com_status_aberto():
    prioridade = Prioridade("Baixa")

    chamado = Chamado(
        titulo="Teste",
        descricao="Descrição",
        prioridade=prioridade
    )

    chamado.status |should| equal_to("Aberto")


def test_f_deve_criar_chamado_com_prioridade():
    prioridade = Prioridade("Alta")

    chamado = Chamado(
        titulo="Urgente",
        descricao="Servidor fora do ar",
        prioridade=prioridade
    )

    chamado.prioridade.valor |should| equal_to("Alta")


def test_f_deve_mudar_status_para_resolvido():
    prioridade = Prioridade("Baixa")

    chamado = Chamado(
        titulo="Bug",
        descricao="Erro qualquer",
        prioridade=prioridade
    )

    chamado.resolver()

    chamado.status |should| equal_to("Resolvido")


def test_f_nao_deve_aceitar_prioridade_invalida():
    with pytest.raises(ValueError):
        Prioridade("Urgente")
