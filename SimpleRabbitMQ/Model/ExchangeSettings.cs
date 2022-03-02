using System.Collections.Generic;

namespace SimpleRabbitMQCore.Model
{
    public class ExchangeSettings
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Durable { get; set; } = true;
        public bool Autodelete { get; set; } = false;
        public IDictionary<string, object> Arguments { get; set; } = null;

        public ExchangeSettings()
        {
        }

        public ExchangeSettings(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}