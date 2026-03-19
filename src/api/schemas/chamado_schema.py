from pydantic import BaseModel, Field

# Esse código garante que ninguém envie um chamado vazio ou errado
class ChamadoCreate(BaseModel):
    titulo: str = Field(..., min_length=5, max_length=100, description="Título do chamado")
    descricao: str = Field(..., min_length=10, description="Descrição detalhada do problema")
    prioridade_valor: str = Field(..., pattern="^(Baixa|Média|Alta)$", description="Valores aceitos: Baixa, Média ou Alta")

    class Config:
        json_schema_extra = {
            "example": {
                "titulo": "Monitor não liga",
                "descricao": "O monitor da recepção parou de funcionar subitamente.",
                "prioridade_valor": "Alta"
            }
        }