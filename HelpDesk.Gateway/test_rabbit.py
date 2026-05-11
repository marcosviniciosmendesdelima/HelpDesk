import pika
import json

# No Docker, o host é o nome do serviço: 'rabbitmq'
# Se rodar fora do Docker (local), use '127.0.0.1'
try:
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host='127.0.0.1', port=5672)
    )
    channel = connection.channel()

    # Criando a Exchange (Automáticamente criada pelo RabbitMQ quando a mensagem é publicada, mas declarada aqui para garantir)
    channel.exchange_declare(exchange='chamado.criado', exchange_type='fanout')

    mensagem = {
        "id": 1,
        "titulo": "Teste do Marcos",
        "email": "marcos@ti.com"
    }

    channel.basic_publish(
        exchange='chamado.criado',
        routing_key='',
        body=json.dumps(mensagem)
    )

    print(" [v] SUCESSO: Mensagem enviada para a exchange 'chamado.criado'!")
    connection.close()
except Exception as e:
    print(f" [x] ERRO: Verifique se o Docker está rodando. Detalhe: {e}")