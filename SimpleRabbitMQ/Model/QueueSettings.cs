using System.Collections.Generic;

namespace SimpleRabbitMQ.Model
{
    public class QueueSettings
    {
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }
        public string QueueName { get; set; }
        public string Description { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
        public int? TTLms { get; set; }

        public IDictionary<string, object> Arguments { get; set; }
    }
}