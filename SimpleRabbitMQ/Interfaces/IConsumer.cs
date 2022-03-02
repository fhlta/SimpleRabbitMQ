using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace SimpleRabbitMQCore
{
    public interface IConsumer<T>
    {
        bool SubscribeConsumer(Action<T> procedure, bool autoAck = true);
    }
}