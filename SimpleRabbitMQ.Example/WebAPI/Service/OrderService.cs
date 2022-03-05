using SimpleRabbitMQCore;
using Domain.Model;
using Serilog;
using System;
using System.Threading.Tasks;

namespace WebAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger _logger;
        private readonly IPublisher<OrderCreateRequest> _publisherMQ;

        public OrderService(ILogger logger, IPublisher<OrderCreateRequest> publisherMQ)
        {
            _logger = logger;
            _publisherMQ = publisherMQ;
        }

        public async Task<OrderCreateResponse> PostOrderServiceAsync(OrderCreateRequest orderDTO)
        {
            try
            {
                for(int i = 0; i < 10; i++)
                {
                    // Sending message to exchange
                    var response = await _publisherMQ.PublishAsync(orderDTO);
                    if (response == false) { Console.WriteLine("Failed to publish message"); }
                }

                return new OrderCreateResponse()
                {
                    protocol = "123"
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}