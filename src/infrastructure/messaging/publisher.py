import pika
import json
import os

def publicar_evento(mensagem: dict):
    # 'rabbitmq' é o nome do serviço definido no seu docker-compose.yml
    rabbitmq_url = os.getenv("RABBITMQ_URL", "amqp://guest:guest@rabbitmq:5672/")
    params = pika.URLParameters(rabbitmq_url)
    
    try:
        connection = pika.BlockingConnection(params)
        channel = connection.channel()
        
        # NOME DA FILA: Alterado para 'ticket_created' para alinhar com o Gateway .NET
        queue_name = 'ticket_created'
        
        # Garante que a fila existe antes de enviar
        channel.queue_declare(queue=queue_name, durable=True)

        # Publica a mensagem
        channel.basic_publish(
            exchange='',
            routing_key=queue_name,
            body=json.dumps(mensagem),
            properties=pika.BasicProperties(
                delivery_mode=2,  # Torna a mensagem persistente (não morre se o Rabbit reiniciar)
                content_type='application/json'
            )
        )
        
        print(f" [Python -> RabbitMQ] SUCESSO: Evento do ticket '{mensagem.get('titulo')}' enviado!")
        
        connection.close()
    except Exception as e:
        print(f" [!] ERRO CRÍTICO ao publicar no RabbitMQ: {e}")