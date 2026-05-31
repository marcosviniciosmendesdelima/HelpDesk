import pika
import json
import uuid

# Configuração da conexão com o RabbitMQ
try:
    # Ajuste o host conforme necessário (se rodar fora do Docker, localhost funciona)
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost', port=5672))
    channel = connection.channel()

    # O nome da fila DEVE ser igual ao que está no seu ChamadoConsumer.cs (fila_tickets)
    queue_name = 'fila_tickets'
    channel.queue_declare(queue=queue_name, durable=True)

    mensagem = {
        "Id": str(uuid.uuid4()),
        "Titulo": "Teste Definitivo",
        "Descricao": "Conexão direta na fila",
        "Prioridade": "Alta",
        "Status": "Aberto"
    }

    # Publicação na fila correta
    channel.basic_publish(
        exchange='',
        routing_key=queue_name,
        body=json.dumps(mensagem),
        properties=pika.BasicProperties(delivery_mode=2) # Mensagem persistente
    )

    print(f" [v] SUCESSO: Mensagem enviada para '{queue_name}'!")
    connection.close()
except Exception as e:
    print(f" [x] ERRO: {e}")