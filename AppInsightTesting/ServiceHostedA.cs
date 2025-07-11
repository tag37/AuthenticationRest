using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppInsightTesting
{
    public class ServiceHostedA : IHostedService
    {
        private readonly ServiceChildA _serviceChildA;
        private readonly ILogger<ServiceHostedA> logger;

        public ServiceHostedA(ServiceChildA serviceChildA, ILogger<ServiceHostedA> logger)
        {
            this._serviceChildA = serviceChildA;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("ServiceHostedA is starting its work.");
                this._serviceChildA.Start();
                logger.LogInformation("ServiceHostedA is Completed its work.");
            }
            catch (Exception ex)
            {
                throw new Exception("Operation failed",ex);
            }
            

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ServiceHostedA is Stopped its work.");
            return Task.CompletedTask;

        }
    }
}
