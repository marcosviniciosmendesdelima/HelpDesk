from dataclasses import dataclass

@dataclass(frozen=True)
class Prioridade:
    # Value Object Prioridade: Imutável conforme o plano
    valor: str

    def __post_init__(self):
        # Validação para garantir que apenas valores permitidos sejam usados
        permitidos = ["Baixa", "Média", "Alta"]
        if self.valor not in permitidos:
            raise ValueError(f"Prioridade inválida. Use: {permitidos}")

    def __str__(self):
        return self.valor