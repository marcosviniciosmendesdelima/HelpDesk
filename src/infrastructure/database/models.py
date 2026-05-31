from sqlalchemy import Column, String, Text, DateTime, UUID
from sqlalchemy.sql import func
from .config import Base
import uuid

class TicketsReadModel(Base):
    # Definimos o nome da tabela exatamente como o C# espera: "TicketsRead"
    # O SQLAlchemy coloca aspas automaticamente se o nome tiver maiúsculas.
    __tablename__ = "TicketsRead"

    # O C# espera um UUID como Primary Key
    id = Column("Id", UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    
    # Colunas em minúsculo conforme o seu banco PostgreSQL
    titulo = Column(String(200), nullable=True)
    descricao = Column(Text, nullable=True)
    prioridade = Column(String(50), nullable=True)
    status = Column(String(50), nullable=True)
    datacriacao = Column(DateTime, default=func.now())