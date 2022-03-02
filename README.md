## Introduction 
Biblioteca RabbitMQ

## Getting Started
1 - Defina no AppSettings as configurações

```
{
  "RabbitMQLibSettings": {
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
2 - Crie um RabbitMQLibSettingCustom

```
public class RabbitMQLibSettingsCustom : RabbitMQLibSettings
{
    public ExchangeSettings OrderExchange { get; set; } # Defina o nome da propriedade de acordo com o valor em appsettings
    public QueueSettings OrderCreateQueue { get; set; } # Defina o nome da propriedade de acordo com o valor em appsettings
}
```

3 - Adicione no model do AppSettings

```
public class AppSettings
{
    public RabbitMQLibSettingsCustom RabbitMQLibSettings { get;set;} # Adicionado
}
```

4 - Instâncie, crie o Exchange, crie as Queues e defina a injeção de dependência.
* Publisher e Consumer são definidor como Singleton, assim como o RabbitMQLib

```
// Instance
var rabbitMQLib = new RabbitMQLib(appSettings.RabbitMQLibSettings, logger);

// Definitions
rabbitMQLib.CreateExchange(appSettings.RabbitMQLibSettings.OrderExchange);
rabbitMQLib.CreateQueue(appSettings.RabbitMQLibSettings.OrderCreateQueue);

// Dependency Injection
services.AddSingleton<IRabbitMQLib, RabbitMQLib>(sp => rabbitMQLib);

// Each DTO must have a unique Publisher and Consumer for its own message
services.AddSingleton<IPublisher<OrderCreateRequest>, Publisher<OrderCreateRequest>>(sp => new Publisher<OrderCreateRequest>(logger, rabbitMQLib, appSettings.RabbitMQLibSettings.OrderCreateQueue));
services.AddSingleton<IConsumer<OrderCreateRequest>, Consumer<OrderCreateRequest>>(sp => new Consumer<OrderCreateRequest>(logger, rabbitMQLib, appSettings.RabbitMQLibSettings.OrderCreateQueue));

services.AddHostedService<OrderWorker>();
```

5 - Registre o consumidor da fila

```
var result = _consumer.SubscribeConsumer(async (order) =>
{
    Console.WriteLine("If you readed this message, the message was successfully received");
});

```

6 - Publique uma mensagem

```
 var response = await _publisherMQ.PublishAsync(orderDTO);
```

## Contribute
[GitHub Pages](https://github.com/fenixrhiulta/)
