using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace SimpleRabbitMQ
{
    public interface IPublisher<T>
    {
        Task<bool> PublishAsync(T request, IBasicProperties properties = null);
    }
}