from fastapi.testclient import TestClient
from src.api.main import app

client = TestClient(app)

def test_criar_chamado_integration():
    response = client.post("/chamados", json={
        "descricao": "Erro de integração"
    })


    assert response.status_code == 201

    # Verifica se os dados estão corretos
    data = response.json()
    assert data["descricao"] == "Erro de integração"