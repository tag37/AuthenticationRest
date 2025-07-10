using System.Security.Authentication;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1
{
    public class HostedServiceWrapper<T> : IHostedService where T : IHostedService
    {
        private readonly T _innerService;
        private readonly ILogger<HostedServiceWrapper<T>> _logger;
        private readonly TelemetryClient _telemetry;

        public HostedServiceWrapper(
            T innerService,
            ILogger<HostedServiceWrapper<T>> logger,
            TelemetryClient telemetry)
        {
            _innerService = innerService;
            _logger = logger;
            _telemetry = telemetry;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting {ServiceName}", typeof(T).Name);

            try
            {
                await _innerService.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while starting {ServiceName}", typeof(T).Name);
                var customException = new AuthenticationException("Custom exception", ex);
                throw customException; // optional: rethrow if you want to crash the app
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping {ServiceName}", typeof(T).Name);
            return _innerService.StopAsync(cancellationToken);
        }
    }

}
