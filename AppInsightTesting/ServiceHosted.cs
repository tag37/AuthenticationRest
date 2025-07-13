using Auth.Exception;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppInsightTesting
{
    public class ServiceHostedA : ServiceBaseHostedA
    {
        private readonly ILogger<ServiceHostedA> logger;

        public ServiceHostedA(ServiceChildA serviceChildA, ILogger<ServiceHostedA> logger, ILogger<ServiceBaseHostedA> baseLogger) : base(serviceChildA, baseLogger)
        {
            this.logger = logger;
        }
    }

    public class ServiceHostedWrapper : IHostedService
    {
        private readonly ServiceHostedA serviceHostedA;

        private readonly ILogger<ServiceHostedWrapper> logger;

        public ServiceHostedWrapper(ServiceHostedA serviceHostedA, ILogger<ServiceHostedWrapper> logger)
        {
            this.serviceHostedA = serviceHostedA;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await serviceHostedA.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Exception from wrapper");
                throw new AuthenticationException("An error occurred while running the application.", ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return serviceHostedA.StopAsync(cancellationToken);
        }
    }
}
