using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace SimpleRabbitMQCore
{
    public interface IPublisher<T>
    {
        Task<bool> PublishAsync(T request, IBasicProperties properties = null);
    }
}