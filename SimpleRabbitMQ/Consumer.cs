using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using SimpleRabbitMQ.Model;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRabbitMQ
{
    public class Consumer<T> : IConsumer<T>
    {
        private readonly QueueSettings _queue;
        private readonly ILogger _logger;
        private readonly IConnection _connection;
        private string _consumerTag;

        public Consumer(ILogger logger, ISimpleRabbitMQService simpleRabbitMQService, QueueSettings queue)
        {
            _queue = queue;
            _logger = logger;
            _connection = simpleRabbitMQService.GetConnection();
        }

        public bool SubscribeConsumer(Action<T> procedure, bool autoAck = true)
        {
            try
            {
                IModel channel = _connection.CreateModel();
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += async (model, ea) =>
                {
                    //await Task.Run(() =>
                    //{
                      
                    //});

                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        procedure.Invoke(JsonConvert.DeserializeObject<T>(message));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Error when attemp to read message from queue");
                    }
                };

                _consumerTag = channel.BasicConsume(queue: _queue.QueueName,
                                       autoAck: autoAck,
                                       consumer: consumer);

                if (string.IsNullOrEmpty(_consumerTag)) return false;

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
