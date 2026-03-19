from fastapi import FastAPI
from fastapi.responses import JSONResponse

app = FastAPI(
    title="Help Desk API",
    version="v1" # Versionamento por URI 
)

# Princípio da Interface Uniforme: Health Check
@app.get("/api/v1/health", status_code=200)
async def health_check():
    return {"status": "online", "message": "API Help Desk pronta para uso"}

# Implementação da RFC 9457 para Erros Padronizados
@app.exception_handler(404)
async def not_found_handler(request, exc):
    return JSONResponse(
        status_code=404,
        content={
            "type": "https://helpdesk.com/probs/not-found",
            "title": "Recurso não encontrado",
            "status": 404,
            "detail": "O endpoint solicitado não existe."
        },
        media_type="application/problem+json" # Tipo de mídia obrigatório da RFC
    )