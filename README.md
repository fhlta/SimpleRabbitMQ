SimpleRabbitMQ - Simplifique o RabbitMQ
========================================

Biblioteca criada para simplificar a interação com o RabbitMQ.

# Features

SimpleRabbitMQ é uma [NuGet library](https://www.nuget.org/packages/SimpleRabbitMQ) que você pode adicionar ao seu projeto e utilizar as interfaces ISimpleRabbitMQ, IPublisher e IConsumer.

### Target

Facilitar a comunicação com o RabbitMQ em APIs. Com ela é possível definir as configurações no AppSettings, como URI, Exchanges e Queues. Publique as mensagens utilizando a abstração "IPublisher" e consuma com o "IConsumer", definidos para cada DTO e com suas respectivas funcionalidades. 

### ISimpleRabbitMQ

```csharp

public SimpleRabbitMQ(SimpleRabbitMQSettings rabbitMQSettings, ILogger logger)

```

Exemplo de uso:

```csharp

var SimpleRabbitMQ = new SimpleRabbitMQ(appSettings.SimpleRabbitMQSettings, logger);
SimpleRabbitMQ.CreateExchange(appSettings.SimpleRabbitMQSettings.OrderExchange);
SimpleRabbitMQ.CreateQueue(appSettings.SimpleRabbitMQSettings.OrderCreateQueue);

```

### IPublisher


```csharp

public Publisher(ILogger logger, ISimpleRabbitMQ simpleRabbitMQ, QueueSettings queue)

```

Exemplo de uso:

```csharp
public async Task<OrderCreateResponse> PostOrderServiceAsync(OrderCreateRequest orderDTO)
{
    try
    {
        var response = await _publisherMQ.PublishAsync(orderDTO);
        if (response == false) 
        { 
            Console.WriteLine("Failed to publish message"); 
            
            // when failed to publish message

            return null;
        }

        return new OrderCreateResponse()
        {
            protocol = "9999999999"
        };
    }
    catch (Exception)
    {
        throw;
    }
}

```

### IConsumer

```csharp

public Consumer(ILogger logger, ISimpleRabbitMQ simpleRabbitMQ, QueueSettings queue)

```

Exemplo de uso:

```csharp

// StartAsync is method from BackgroundService - using Microsoft.Extensions.Hosting;

public override Task StartAsync(CancellationToken cancellationToken)
{
    // Subscribe consumer to queue
    var result = _consumer.SubscribeConsumer(async (order) =>
    {
        // Occurs when the message is received
        Console.WriteLine("Leitura de mensagem da fila do RabbitMQ realizada com sucesso!");
    });

    if (result) _logger.Information("Consumer subscribed");

    return base.StartAsync(cancellationToken);
}
```

## Getting Started

1 - Defina no seu appsettings.json

```csharp
{
  "SimpleRabbitMQSettings": {
    "Uri": "amqp://user:password@host:port/", # altere conforme o seu ambiente
    
    # Exchange Setting
    "OrderExchange": {
      "Name": "order-ex",
      "Type": "topic"
    },

    # Queue Setting
    "OrderCreateQueue": {
      "QueueName": "order-create-qu",
      "ExchangeName": "order-ex",
      "RoutingKey": "order-create-rk"
    }
  }
}
```
2 - Crie um SimpleRabbitMQSettingsCustom

```csharp
public class SimpleRabbitMQSettingsCustom : SimpleRabbitMQSettings
{
    public ExchangeSettings OrderExchange { get; set; } # Exchange definida em appsettings
    public QueueSettings OrderCreateQueue { get; set; } # Queue definida em appsettings
}
```

3 - Adicione no seu model do AppSettings.cs a propriedade

```csharp
public class AppSettings
{
    public SimpleRabbitMQSettingsCustom SimpleRabbitMQSettings { get;set;} 
}
```

4 - No StartUp.cs, no método "ConfigureServices"

```csharp
// Cria a instância
var rabbitMQLib = new RabbitMQLib(appSettings.RabbitMQLibSettings, logger);

// Cria o Exchange, caso não exista
rabbitMQLib.CreateExchange(appSettings.RabbitMQLibSettings.OrderExchange);

// Cria a Queue, caso não exista
rabbitMQLib.CreateQueue(appSettings.RabbitMQLibSettings.OrderCreateQueue);

// Injeção de dependência
// Cada DTO tem sua injeção de dependência através do IPublisher e IConsumer

services.AddSingleton<IRabbitMQLib, RabbitMQLib>(sp => rabbitMQLib);
services.AddSingleton<IPublisher<OrderCreateRequest>, Publisher<OrderCreateRequest>>(sp => new Publisher<OrderCreateRequest>(logger, rabbitMQLib, appSettings.RabbitMQLibSettings.OrderCreateQueue));
services.AddSingleton<IConsumer<OrderCreateRequest>, Consumer<OrderCreateRequest>>(sp => new Consumer<OrderCreateRequest>(logger, rabbitMQLib, appSettings.RabbitMQLibSettings.OrderCreateQueue));

services.AddHostedService<OrderWorker>();
```

5 - Registrando o consumidor da fila

```csharp
var result = _consumer.SubscribeConsumer(async (order) =>
{
    Console.WriteLine("Leitura de mensagem da fila do RabbitMQ realizada com sucesso!");
});

```

6 - Publique uma mensagem

```csharp
//_publisherMQ é uma referência ao IPublisher injetado.
var response = await _publisherMQ.PublishAsync(orderDTO);
```

## Performance

Soon...

## Next Features

- QOS
- ACK
- Performance test
- Stress Test

## Contribute
[Fenix Rhiulta](https://github.com/fenixrhiulta/)
