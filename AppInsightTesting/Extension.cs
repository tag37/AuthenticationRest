using System.Diagnostics;
using Auth.Exception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace AppInsightTesting
{
    public class MyActivitySource
    {
        public static string Name = nameof(MyActivitySource);
        public static ActivitySource Instance = new(Name);
    }

    public static class Extension
    {
        public static async Task RunManageAsync<T>(this IHost host, ILogger<T> logger, CancellationToken token = default)
        {
            try
            {
                await host.StartAsync(token).ConfigureAwait(false);
                await host.WaitForShutdownAsync(token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //var telemetryClient = host.Services.GetRequiredService<TelemetryClient>();
                // var logger = host.Services.GetRequiredService<ILogger<Program>>();
                var exception = new AuthenticationException("An error occurred while running the application.", ex);
                logger.LogError(exception, "Unhandled exception caught in Main");
                //WorkingUsingActivity(host, exception);

                //var exceptionTelemetry = new ExceptionTelemetry(exception);
                //telemetryClient.TrackException(exceptionTelemetry);
                //await telemetryClient.FlushAsync(CancellationToken.None);
                //activity?.AddTag("error", "true")
                //.AddTag("error.kind", exception.GetType().Name)
                //.AddTag("error.message", exception.Message)
                //.AddTag("error.stack", exception.StackTrace);

                throw exception;
            }
            finally
            {
                if (host is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    host.Dispose();
                }
            }
        }

        private static void WorkingUsingActivity(IHost host, AuthenticationException exception)
        {
            //var activity = MyActivitySource.Instance.StartActivity(MyActivitySource.Name);

            // Still within scope of an active host
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            //var assembly = typeof(Program).Assembly;
            //var name = assembly.GetName().Name;
            //var version = assembly.GetName().Version?.ToString();

            //activity?.SetStatus(ActivityStatusCode.Error);
            //activity?.AddException(exception);
            //activity?.SetTag("assembly", assembly);
            //activity?.SetTag("innermostAssembly", assembly);
            //activity?.SetTag("error", true);

            logger.LogError(exception, "Unhandled exception caught in Main");
        }
    }
}


