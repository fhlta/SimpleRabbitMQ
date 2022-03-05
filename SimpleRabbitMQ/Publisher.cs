using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;
using SimpleRabbitMQCore.Model;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRabbitMQCore
{
    public class Publisher<T> : IPublisher<T>
    {
        private readonly IModel _channel;
        private readonly string _exchange;
        private readonly string _routingKey;
        private readonly ILogger _logger;
        private readonly ISimpleRabbitMQ _simpleRabbitMQ;

        public readonly Guid _id = new Guid();

        public Publisher(ILogger logger, ISimpleRabbitMQ simpleRabbitMQ, QueueSettings queue)
        {
           
            _exchange = queue.ExchangeName;
            _routingKey = queue.RoutingKey;
            _logger = logger;

            _simpleRabbitMQ = simpleRabbitMQ;

            if (simpleRabbitMQ.IsConnected)
            {
                _channel = simpleRabbitMQ.GetConnection().CreateModel();
            };
        }

        public async Task<bool> PublishAsync(T request, IBasicProperties properties = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (_simpleRabbitMQ.IsConnected == false)
                    {
                        _logger.Error($"[RabbitMQ.Publisher] RabbitMQ connection problem. Connection not found.");
                        return false;
                    }

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

                    _logger.Information("[RabbitMQ.Publisher] Published message");

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
