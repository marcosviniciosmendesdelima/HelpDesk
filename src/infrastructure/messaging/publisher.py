import pika
import json

def publicar_evento(evento: dict):
    # 'rabbitmq' é o nome do container definido no seu docker-compose
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq'))
    channel = connection.channel()
    
    # O nome da fila DEVE ser 'fila_tickets'
    channel.queue_declare(queue='fila_tickets', durable=True)
    
    channel.basic_publish(
        exchange='',
        routing_key='fila_tickets', # Endereço correto
        body=json.dumps(evento),
        properties=pika.BasicProperties(delivery_mode=2)
    )
    connection.close()