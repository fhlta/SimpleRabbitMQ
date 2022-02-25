using RabbitMQ.Client;
using SimpleRabbitMQ.Model;
using System;
using System.Threading.Tasks;

namespace SimpleRabbitMQ
{
    public interface ISimpleRabbitMQService
    {
        IConnection GetConnection();

        public void CreateQueue(QueueSettings queue);

        Task Subscribe(string queueName, Action<string> procedure);

        Task<T> EnqueueRPCAsync<T>(object request, string routingKey);

        Task QueueConsumeEventRPC<T>(string queueName, Func<string, Task<T>> procedure);
    }
}