import pika
import json
import uuid

try:
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost', port=5672))
    channel = connection.channel()
    channel.queue_declare(queue='fila_tickets', durable=True)
    mensagem = {
        "Id": str(uuid.uuid4()),
        "Titulo": "Teste Direto",
        "Descricao": "Conexão e gravação OK",
        "Prioridade": "Alta",
        "Status": "Aberto"
    }
    channel.basic_publish(
        exchange='',
        routing_key='fila_tickets',
        body=json.dumps(mensagem),
        properties=pika.BasicProperties(delivery_mode=2)
    )
    print(" [v] SUCESSO: Mensagem enviada diretamente para 'fila_tickets'!")
    connection.close()
except Exception as e:
    print(f" [x] ERRO: {e}")
