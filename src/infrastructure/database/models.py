from sqlalchemy import Column, Integer, String, Text, DateTime
from datetime import datetime
from .config import Base

class ChamadoModel(Base):
    __tablename__ = "chamados"

    id = Column(Integer, primary_key=True, index=True)
    titulo = Column(String(100), nullable=False)
    descricao = Column(Text, nullable=False)
    status = Column(String(20), default="aberto")
    data_criacao = Column(DateTime, default=datetime.utcnow)