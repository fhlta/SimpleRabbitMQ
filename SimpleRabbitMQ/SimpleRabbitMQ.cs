using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using SimpleRabbitMQCore.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRabbitMQCore
{
    /// <summary>
    /// Simple library to interact with RabbitMQ instance.
    /// </summary>
    public class SimpleRabbitMQ : ISimpleRabbitMQ
    {
        private readonly SimpleRabbitMQSettings _rabbitMQSettings;
        private readonly ILogger _logger;
        private IConnection _conn;
        private IModel _channel;
        private ConnectionFactory _connectionFactory;
        public bool IsConnected { get; private set; }

        public SimpleRabbitMQ(SimpleRabbitMQSettings rabbitMQSettings, ILogger logger)
        {
            _logger = logger;
            _rabbitMQSettings = rabbitMQSettings;

            if (rabbitMQSettings != null)
            {
                CreateConnection(rabbitMQSettings.Uri);
            }
        }

        #region Connection

        public void CreateConnection(string connectionString)
        {
            try
            {
                _connectionFactory = new ConnectionFactory() { Uri = new Uri(connectionString) };
                _conn = _connectionFactory.CreateConnection();
                _channel = _conn.CreateModel();
                IsConnected = true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Can't connect to the RabbitMQ server");
                IsConnected = false;
            }
        }

        public IConnection GetConnection()
        {
            try
            {
                if (_conn == null) throw new Exception("[SimpleRabbitMQ] No connection found");
                return _conn;
            }
            catch (Exception e)
            {
                _logger.Error(e, "");
            }

            return null;
        }

        #endregion Connection

        #region Exchange

        public void CreateExchange(ExchangeSettings exchange)
        {
            try
            {
                _channel.ExchangeDeclarePassive(exchange.Name);
                _logger.Warning("[SimpleRabbitMQ] Fail to attempt create exchange. Exchange already exits");
                return;
            }
            catch
            {
                _channel = _conn.CreateModel();
            }

            try
            {
                _channel.ExchangeDeclare(exchange.Name, exchange.Type, exchange.Durable, exchange.Autodelete, exchange.Arguments);
                _logger.Information($"[SimpleRabbitMQ] The exchange {exchange.Name} was created.");
            }
            catch (Exception e)
            {
                _logger.Error(e, "[SimpleRabbitMQ] Fail to attempt create exchange");
            }
        }

        public void CreateExchange(string exchangeName, string type)
        {
            try
            {
                _channel.ExchangeDeclarePassive(exchangeName);
                _logger.Warning("[SimpleRabbitMQ] Fail to attempt create exchange. Exchange already exits");
                return;
            }
            catch
            {
                _channel = _conn.CreateModel();
            }

            try
            {
                _channel.ExchangeDeclare(exchangeName, type, true);
                _logger.Information($"[SimpleRabbitMQ] The exchange {exchangeName} was created.");

            }
            catch (Exception e)
            {
                _logger.Error(e, "[SimpleRabbitMQ] Fail to attempt create exchange");
            }
        }

        public void CreateExchange(string exchange, string type, bool durable = true, bool autodelete = false, IDictionary<string, object> arguments = null)
        {
            try
            {
                _channel.ExchangeDeclarePassive(exchange);
                _logger.Warning("[SimpleRabbitMQ] Fail to attempt create exchange. Exchange already exits");
                return;
            }
            catch
            {
                _channel = _conn.CreateModel();
            }

            try
            {
                _channel.ExchangeDeclare(exchange, type, durable, autodelete, arguments);
            }
            catch (Exception e)
            {
                _logger.Error(e, "[SimpleRabbitMQ] Fail to attempt create exchange");
            }
        }

        #endregion Exchange

        #region Queue

        public void CreateQueue(string queueName, string exchangeName, string routingKey)
        {

            try
            {
                _channel.QueueDeclarePassive(queueName);
                _logger.Warning("[SimpleRabbitMQ] Fail to attempt create queue. Queue already exits");
                return;
            }
            catch
            {
                _channel = _conn.CreateModel();
            }

            try
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _channel.QueueBind(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey
                    );

                _logger.Information($"[SimpleRabbitMQ] The queue {queueName} was created.");

            }
            catch (Exception e)
            {
                _logger.Error(e, "[SimpleRabbitMQ] Fail to attempt create queue");
            }
        }

        public void CreateQueue(QueueSettings queue)
        {
            try
            {
                _channel.QueueDeclarePassive(queue.QueueName);
                _logger.Warning("[SimpleRabbitMQ] Fail to attempt create queue. Queue already exits");
                return;
            }
            catch
            {
                _channel = _conn.CreateModel();
            }

            try
            {
                _channel.QueueDeclare(
                       queue: queue.QueueName,
                       durable: queue.Durable,
                       exclusive: queue.Exclusive,
                       autoDelete: queue.AutoDelete,
                       arguments: queue.Arguments
                       );

                _channel.QueueBind(
                    queue: queue.QueueName,
                    exchange: queue.ExchangeName,
                    routingKey: queue.RoutingKey
                    );

                _logger.Information($"[SimpleRabbitMQ] The queue {queue.QueueName} was created.");

            }
            catch (Exception e)
            {
                _logger.Error(e, "[SimpleRabbitMQ] Fail to attempt create queue");
            }
        }

        #endregion Queue

        public async Task Subscribe(string queueName, Action<string> procedure)
        {
            await Task.Run(() =>
            {
                try
                {
                    IModel channel = _conn.CreateModel();
                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        try
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            procedure.Invoke(message);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"[QueueConsumeEvent.Received] Error when attemp to read message from queue: {queueName}");
                        }
                    };

                    channel.BasicConsume(queue: queueName,
                                           autoAck: true,
                                           consumer: consumer);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"[QueueConsumeEvent] Error when attempt create consumer of queue: {queueName}");
                }
            });
        }
    }
}