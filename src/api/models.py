from sqlalchemy import Column, Integer, String, Text, DateTime
from datetime import datetime
from .database import Base

class Chamado(Base):
    __tablename__ = "chamados"

    id = Column(Integer, primary_key=True, index=True)
    titulo = Column(String(100), nullable=False)
    descricao = Column(Text, nullable=False)
    prioridade = Column(String(20), default="Media") # Baixa, Media, Alta
    status = Column(String(20), default="Aberto")    # Aberto, Em Atendimento, Concluido
    data_criacao = Column(DateTime, default=datetime.utcnow)