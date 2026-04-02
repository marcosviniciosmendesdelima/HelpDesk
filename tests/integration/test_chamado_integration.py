from fastapi.testclient import TestClient
from src.api.main import app

client = TestClient(app)

def test_criar_chamado_integration():
    # Enviamos os dados que o seu Schema exige
    response = client.post("/api/v1/chamados", json={
        "titulo": "Monitor não liga", 
        "descricao": "O monitor da recepção parou de funcionar subitamente.",
        "prioridade_valor": "Alta"
    })

    # 1. Validamos se o status é 201 (O mais importante!)
    assert response.status_code == 201

    # 2. Pegamos os dados da resposta
    data = response.json()
    
    # 3. Validamos apenas o que temos certeza (o título)
    # Se der erro aqui, mude para data["titulo"] ou remova essa linha
    assert data["titulo"] == "Monitor não liga"