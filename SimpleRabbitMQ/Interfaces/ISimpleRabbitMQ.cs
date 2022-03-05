using SimpleRabbitMQCore.Model;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleRabbitMQCore
{
    public interface ISimpleRabbitMQ
    {
        void CreateConnection(string connectionString);
        void CreateExchange(ExchangeSettings exchange);
        void CreateExchange(string exchange, string type);
        void CreateExchange(string exchange, string type, bool durable = true, bool autodelete = false, IDictionary<string, object> arguments = null);
        void CreateQueue(QueueSettings queue);
        void CreateQueue(string queueName, string exchangeName, string routingKey);
        IConnection GetConnection();
        Task Subscribe(string queueName, Action<string> procedure);
        bool IsConnected { get;}

    }
}