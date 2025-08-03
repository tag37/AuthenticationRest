using System;
using System.Diagnostics;
using Auth.Exception;
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
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: "1.11")).AddOtlpExporter();
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
            //builder.Services.AddScoped<ServiceHostedA>();

            builder.Services.AddScoped<ServiceChildA>();
            builder.Services.AddScoped<ServiceChildB>();
            var application = builder.Build();

            // using var activity = LogActivitySource.StartActivity("AppInsightTesting.Program");

            try
            {
                var logger = application.Services.GetRequiredService<ILogger<Program>>();
                await application.RunManageAsync(logger);
            }
            catch (Exception ex)
            {
                //var exception = new AuthenticationException("An error occurred while running the application.", ex);

                //var activity = MyActivitySource.Instance.StartActivity(MyActivitySource.Name);
                //var assembly = typeof(Program).Assembly;
                //var name = assembly.GetName().Name;
                //var version = assembly.GetName().Version?.ToString();
                //activity?.SetStatus(ActivityStatusCode.Error);
                //activity?.AddException(exception);
                //activity?.SetTag("assembly", assembly);
                //activity?.SetTag("innermostAssembly", assembly);
                //activity?.SetTag("error", true);

                //throw ex;
                //Environment.Exit(1);
            }

        }
    }
}
