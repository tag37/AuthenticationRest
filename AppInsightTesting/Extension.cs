using System.Diagnostics;
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
        public static async Task RunManageAsync(this IHost host, CancellationToken token = default)
        {
            try
            {
                await host.StartAsync(token).ConfigureAwait(false);
                await host.WaitForShutdownAsync(token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exception = new Auth.Exception.AuthenticationException("An error occurred while running the application.", ex);
                using var activity = MyActivitySource.Instance.StartActivity(MyActivitySource.Name);

                // Still within scope of an active host
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                var tracerProvider = host.Services.GetService<TracerProvider>();


                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.AddException(exception);
                
                //activity?.AddTag("error", "true")
                //.AddTag("error.kind", exception.GetType().Name)
                //.AddTag("error.message", exception.Message)
                //.AddTag("error.stack", exception.StackTrace);
                activity?.Stop();

                tracerProvider?.ForceFlush();
                logger.LogError(ex, "Unhandled exception caught in Main");
                await Task.Delay(5000);
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
    }
}


