using Microsoft.Extensions.Logging;

namespace AppInsightTesting
{
    public class ServiceChildB
    {
        private readonly ILogger<ServiceChildB> logger;

        public ServiceChildB(ILogger<ServiceChildB> logger)
        {
            this.logger = logger;
        }

        public void Start()
        {
            // Simulate some work
            logger.LogInformation("ServiceChildB is starting its work.");
            try
            {
                // Simulate an operation that could fail
                throw new InvalidOperationException("An error occurred in ServiceChildB.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred in ServiceChildB.");
                throw new InvalidOperationException("Catch - ServiceChildB - Exception"); //Re - throw the exception to be handled by the caller
            }
        }
    }
}
