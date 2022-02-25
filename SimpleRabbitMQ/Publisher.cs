using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;
using SimpleRabbitMQ.Model;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRabbitMQ
{
    public class Publisher<T> : IPublisher<T>
    {
        private readonly IModel _channel;
        private readonly string _exchange;
        private readonly string _routingKey;
        private readonly ILogger _logger;
        public readonly Guid _id = new Guid();

        public Publisher(ILogger logger, ISimpleRabbitMQService simpleRabbitMQService, QueueSettings queue)
        {
            _channel = simpleRabbitMQService.GetConnection().CreateModel();
            _exchange = queue.ExchangeName;
            _routingKey = queue.RoutingKey;
            _logger = logger;

            simpleRabbitMQService.CreateQueue(queue);
        }

        public async Task<bool> PublishAsync(T request, IBasicProperties properties = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string requestSerialized = JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);

                    var body = Encoding.UTF8.GetBytes(requestSerialized);

                    if (properties == null) properties = _channel.CreateBasicProperties();
                    properties.Persistent = true;

                    _channel.ConfirmSelect();

                    _channel.BasicPublish(exchange: _exchange,
                                         routingKey: _routingKey,
                                         basicProperties: properties,
                                         body: body);

                    _channel.WaitForConfirmsOrDie();

                    _logger.Information("Published message");

                    return true;
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error");
                    return false;
                }
            });
        }
    }
}
