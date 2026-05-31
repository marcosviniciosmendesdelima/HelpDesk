from sqlalchemy import Column, String, Text, DateTime, UUID
from sqlalchemy.sql import func
from .database import Base
import uuid

class Chamado(Base):
    # Nome da tabela ajustado para o padrão que o .NET Gateway consome
    __tablename__ = "TicketsRead"

    # id mapeado como 'Id' (maiúsculo) e tipo UUID como no Postgres
    id = Column("Id", UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    
    # Colunas em minúsculo, conforme o schema do banco
    titulo = Column(String(200), nullable=False)
    descricao = Column(Text, nullable=False)
    prioridade = Column(String(50), default="Media")
    status = Column(String(50), default="Aberto")
    
    # datacriacao segue o nome da coluna que o .NET espera
    datacriacao = Column(DateTime, default=func.now())