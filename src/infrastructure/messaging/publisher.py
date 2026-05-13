import pika
import json
import os

def publicar_evento(mensagem: dict):
    # Padrão: usa o nome do serviço no docker ou localhost se estiver fora
    rabbitmq_url = os.getenv("RABBITMQ_URL", "amqp://guest:guest@localhost:5672/")
    params = pika.URLParameters(rabbitmq_url)
    
    try:
        connection = pika.BlockingConnection(params)
        channel = connection.channel()
        channel.queue_declare(queue='fila_tickets', durable=True)

        channel.basic_publish(
            exchange='',
            routing_key='fila_tickets',
            body=json.dumps(mensagem),
            properties=pika.BasicProperties(delivery_mode=2)
        )
        
        print(f" [x] Evento enviado para o RabbitMQ: {mensagem['id']}")
        connection.close()
    except Exception as e:
        print(f" [!] Erro ao publicar no RabbitMQ: {e}")