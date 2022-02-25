using SimpleRabbitMQ.Model;

namespace Domain.Model
{
    public class SimpleRabbitMQSettingsCustom : SimpleRabbitMQSettings
    {
        public ExchangeSettings OrderExchange { get; set; }
        public QueueSettings OrderCreateQueue { get; set; }
    }
}
