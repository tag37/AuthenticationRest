using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppInsightTesting
{
    public class ServiceBaseHostedA : IHostedService
    {
        private readonly ServiceChildA _serviceChildA;
        private readonly ILogger<ServiceBaseHostedA> logger;

        public ServiceBaseHostedA(ServiceChildA serviceChildA, ILogger<ServiceBaseHostedA> logger)
        {
            this._serviceChildA = serviceChildA;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("ServiceBaseHostedA is starting its work.");
                this._serviceChildA.Start();
                logger.LogInformation("ServiceBaseHostedA is Completed its work.");
            }
            catch (Exception ex)
            {
                throw new Exception("Operation failed", ex);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ServiceBaseHostedA is Stopped its work.");
            return Task.CompletedTask;

        }
    }
}
