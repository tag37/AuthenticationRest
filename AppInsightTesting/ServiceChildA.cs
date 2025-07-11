using Microsoft.Extensions.Logging;

namespace AppInsightTesting
{
    public class ServiceChildA
    {
        private readonly ServiceChildB serviceChildB;
        private readonly ILogger<ServiceChildA> logger;

        public ServiceChildA(ServiceChildB serviceChildB, ILogger<ServiceChildA> logger)
        {
            this.serviceChildB = serviceChildB;
            this.logger = logger;
        }

        public void Start()
        {
            logger.LogInformation("ServiceChildA is starting its work.");
            try
            {
                // Simulate an operation that could fail
                serviceChildB.Start();
                logger.LogInformation("ServiceChildA is completed its work.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred in ServiceChildA Cought from serviceChildB.");
                throw new Exception("Catch - ServiceChildA - Exception"); //Re - throw the exception to be handled by the caller
            }
        }
    }
}
