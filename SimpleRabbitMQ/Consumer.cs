using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using SimpleRabbitMQCore.Model;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRabbitMQCore
{
    public class Consumer<T> : IConsumer<T>
    {
        private readonly QueueSettings _queue;
        private readonly ILogger _logger;
        private readonly ISimpleRabbitMQ _simpleRabbitMQ;

        public Consumer(ILogger logger, ISimpleRabbitMQ simpleRabbitMQ, QueueSettings queue)
        {
            _queue = queue;
            _logger = logger;
            _simpleRabbitMQ = simpleRabbitMQ;
        }

        public bool SubscribeConsumer(Action<T> procedure, bool autoAck = true)
        {
            try
            {
                if (_simpleRabbitMQ.IsConnected == false)
                {
                    _logger.Error($"[RabbitMQ.Consumer] RabbitMQ connection problem. Connection not found.");
                    return false;
                }

                IModel channel = _simpleRabbitMQ.GetConnection().CreateModel();
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += async (model, ea) =>
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            _logger.Information($"[RabbitMQ.Consumer] Reading message...");

                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            procedure.Invoke(JsonConvert.DeserializeObject<T>(message));
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Error when attemp to read message from queue");
                        }
                    });
                };

                string consumerTag = channel.BasicConsume(queue: _queue.QueueName,
                                       autoAck: autoAck,
                                       consumer: consumer);

                if (string.IsNullOrEmpty(consumerTag)) return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[QueueConsumeEvent] Error when attempt create consumer of queue");
                return false;
            }
        }
    }
}
