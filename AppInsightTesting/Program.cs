using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AppInsightTesting
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            OpenTelemetrySdk? openTelemetrySdk = null;
            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var serviceName = "Tushar-Service";

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName)).AddOtlpExporter();
                options.AddAzureMonitorLogExporter(option => option.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);
                options.IncludeScopes = true; // Optional: include scopes in logs
                options.IncludeFormattedMessage = true; // Optional: include formatted message in logs
            });

            var telemtryBuilder = builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName));

            telemtryBuilder.WithTracing(tracing =>
            {
                tracing.AddHttpClientInstrumentation(option => option.RecordException = true)
                .AddSource("Auth.BackgroundService")
                .AddSource("AppInsightTesting.Program")
                .AddSource("ConsoleApp1")
                .AddSource(MyActivitySource.Name)
                .AddAspNetCoreInstrumentation()
                .AddAzureMonitorTraceExporter();


            }).WithMetrics(matrix =>
            {
                matrix.AddAzureMonitorMetricExporter(option => option.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);
            });
            //builder.Services.AddApplicationInsightsTelemetryWorkerService(option =>
            //{
            //    option.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            //}
            //);
            builder.Services.AddHostedService<ServiceHostedA>();
            builder.Services.AddScoped<ServiceChildA>();
            builder.Services.AddScoped<ServiceChildB>();
            var application = builder.Build();

            // using var activity = LogActivitySource.StartActivity("AppInsightTesting.Program");

            try
            {
                await application.RunManageAsync();
            }
            catch (Exception)
            {
                Environment.Exit(1);
            }

        }
    }
}
