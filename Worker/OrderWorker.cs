using Domain.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using SimpleRabbitMQ;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Worker
{
    public class OrderWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConsumer<OrderCreateRequest> _consumer;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OrderWorker(
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory,
            IConsumer<OrderCreateRequest> consumer
            )
        {
            _logger = logger;
            _consumer = consumer;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var result = _consumer.SubscribeConsumer(async (order) =>
            {
                Console.WriteLine("# START");

                await Task.Delay(5000);

                Console.WriteLine("@ END");

            });

            if (result) _logger.Information("Consumer subscribed");

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.Information("[Worker] Starting Worker running at: {time}", DateTimeOffset.Now);
            }
            catch (Exception e)
            {
                _logger.Error(e, "[Worker] Exception in SetQueueConsume");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Information("[Worker] PickupCreateWorker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
};