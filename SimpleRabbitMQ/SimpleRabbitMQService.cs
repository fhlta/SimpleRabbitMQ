using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using SimpleRabbitMQ.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRabbitMQ
{
    public class SimpleRabbitMQService : ISimpleRabbitMQService
    {
        private readonly SimpleRabbitMQSettings _simpleRabbitMQSettings;
        private readonly ILogger _logger;

        public bool IsConnected = false;
        private IConnection _conn;
        private IModel _channel;
        private ConnectionFactory _connectionFactory;

        public SimpleRabbitMQService(
            SimpleRabbitMQSettings simpleRabbitMQSettings,
            ILogger logger
            )
        {
            _logger = logger;
            _simpleRabbitMQSettings = simpleRabbitMQSettings;

            if (simpleRabbitMQSettings != null)
            {
                CreateConnection(simpleRabbitMQSettings.Uri);
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
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot connect RabbitMQ server");
            }
        }

        public IConnection GetConnection()
        {
            try
            {
                if (_conn == null) throw new Exception("Connection not found");
                return _conn;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion Connection

        #region Publisher

        //public Publisher<T> CreatePublisher<T>(string exchange, string routingKey)
        //{
        //    try
        //    {
        //        return new Publisher<T>(_logger, exchange, routingKey);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        #endregion Publisher

        #region Consumer

        //public Consumer<T> CreateConsumer<T>(string queue, string routingKey)
        //{
        //    try
        //    {
        //        return new Consumer<T>(logger: _logger, connection: _conn, queueName: queue);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        #endregion Consumer

        #region Exchange

        public void CreateExchange(ExchangeSettings exchange)
        {
            try
            {
                _channel.ExchangeDeclare(exchange.Name, exchange.Type, exchange.Durable, exchange.Autodelete, exchange.Arguments);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateExchange(string exchange, string type)
        {
            try
            {
                _channel.ExchangeDeclare(exchange, type, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateExchange(string exchange, string type, bool durable = true, bool autodelete = false, IDictionary<string, object> arguments = null)
        {
            try
            {
                _channel.ExchangeDeclare(exchange, type, durable, autodelete, arguments);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Exchange

        #region Queue

        public void CreateQueue(string queueName, string exchangeName, string routingKey)
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
        }

        public void CreateQueue(QueueSettings queue)
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
        }

        #endregion Queue

        public bool CreateBind()
        {
            return false;
        }

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

        public async Task<T> EnqueueRPCAsync<T>(object request, string routingKey)
        {
            var task = Task.Run(() =>
            {
                string requestSerialized = JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);

                IBasicProperties props = _channel.CreateBasicProperties();
                props.CorrelationId = Guid.NewGuid().ToString();
                props.ReplyTo = _channel.QueueDeclare(autoDelete: true, arguments: new Dictionary<string, object>()
                {
                    {"x-expires", 30000}
                }).QueueName;

                var consumer = new EventingBasicConsumer(_channel);

                bool waitingResponse = true;
                T responseMessage = default(T);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var responseString = Encoding.UTF8.GetString(body);
                    responseMessage = JsonConvert.DeserializeObject<T>(responseString);
                    waitingResponse = false;
                };

                _channel.BasicConsume(
                 consumer: consumer,
                 queue: props.ReplyTo,
                 autoAck: true);

                Console.WriteLine($"Sending message and wait response from worker...");
                var body = Encoding.UTF8.GetBytes(requestSerialized);

                //_channel.BasicPublish(exchange: _simpleRabbitMQSettings.ExchangeName,
                //                     routingKey: routingKey,
                //                     basicProperties: props,
                //                     body: body);

                while (waitingResponse) continue;

                return responseMessage;
            });

            //Time out definido para 10 segundos
            if (task.Wait(TimeSpan.FromSeconds(10)))
                return task.Result;
            else
                throw new TimeoutException("[EnqueueRPCAsync] Timed out, the worker did not respond in time.");
        }

        public async Task QueueConsumeEventRPC<T>(string queueName, Func<string, Task<T>> procedure)
        {
            await Task.Run(() =>
            {
                try
                {
                    IModel channel = _conn.CreateModel();
                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += async (model, ea) =>
                    {
                        try
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            IBasicProperties props = ea.BasicProperties;

                            //RPC message
                            if (props.CorrelationId != null)
                            {
                                var replyProps = channel.CreateBasicProperties();
                                replyProps.CorrelationId = props.CorrelationId;

                                var result = await procedure.Invoke(message);

                                string requestSerialized = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
                                var responseBytes = Encoding.UTF8.GetBytes(requestSerialized);

                                channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                basicProperties: replyProps, body: responseBytes);
                            }
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
                    _logger.Error(ex, $"[QueueConsumeEvent] Error when attempt to create consumer of queue: {queueName}");
                }
            });
        }
    }
}