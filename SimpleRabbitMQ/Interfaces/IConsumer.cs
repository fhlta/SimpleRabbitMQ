using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace SimpleRabbitMQ
{
    public interface IConsumer<T>
    {
        bool SubscribeConsumer(Action<T> procedure, bool autoAck = true);
    }
}