// See https://aka.ms/new-console-template for more information
using System;
using System.Security.Authentication;
using Auth.BackgroundService;
using Azure.Monitor.OpenTelemetry.Exporter;
using ConsoleApp1;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
//Console.WriteLine("Hello, World!");
//await new ChunkedDataPipeline().RunAsync();

namespace CustomeException
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            // Configure resource (useful for logs, metrics, tracing)
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService("CustomeException.Program");

            // OPTIONAL: a named meter for your custom metrics

            // Add the required using directive for OpenTelemetry.Extensions.Logging  


            // Update the logging configuration to ensure the extension method is recognized  

            // 👇 Setup OpenTelemetry Metrics
            var serviceName = "Tushar-Service";

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName)).AddOtlpExporter();
                options.AddAzureMonitorLogExporter(option => option.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);
                options.IncludeScopes = true; // Optional: include scopes in logs
                options.IncludeFormattedMessage = true; // Optional: include formatted message in logs

            });

            var telemtryBuilder = builder.Services.AddOpenTelemetry().ConfigureResource(resource => resource.AddService(serviceName));

            telemtryBuilder.WithTracing(tracing =>
            {
                tracing.AddHttpClientInstrumentation(option => option.RecordException = true)
                .AddSource("Auth.BackgroundService")
                .AddSource("CustomeException.Program")
                .AddSource("ConsoleApp1")
                .AddSource("CustomeException")
                .AddAspNetCoreInstrumentation()
                .AddAzureMonitorTraceExporter();
            }).WithMetrics(matrix =>
            {
                matrix.AddAzureMonitorMetricExporter(option => option.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);

            });

            // Add your background services, telemetry, etc.

            builder.Services.AddApplicationInsightsTelemetryWorkerService(option =>
            {
                option.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            }
            );
            builder.Services.AddSingleton<ChunkedDataPipeline>();

            builder.Services.AddSingleton<IHostedService>(provider =>
            {
                var actualService = provider.GetRequiredService<ChunkedDataPipeline>();
                var logger = provider.GetRequiredService<ILogger<HostedServiceWrapper<ChunkedDataPipeline>>>();
                var telemetryClient = provider.GetRequiredService<TelemetryClient>();
                return new HostedServiceWrapper<ChunkedDataPipeline>(actualService, logger, telemetryClient);
            });
            var application = builder.Build();

            var telemetryClient = application.Services.GetRequiredService<TelemetryClient>();

            //var logger = application.Services.GetRequiredService<ILogger<Program>>();
            try
            {
                await application.RunAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

            //try
            //{
            //    telemetryClient.TrackTrace("This telemetry message - Application Starting");
            //    await application.RunAsync();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Connection string: " + builder.Configuration["ApplicationInsights:ConnectionString"]);
            //    var customException = new AuthenticationException("Custom exception", ex);
            //    telemetryClient.TrackException(customException);
            //    telemetryClient.TrackTrace("Custom exception occurred during host shutdown");
            //    telemetryClient.Flush();
            //    await Task.Delay(5000);
            //    Environment.Exit(1);
            //}

            //AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            //{
            //    var ex = (Exception)eventArgs.ExceptionObject;
            //    telemetryClient.TrackException(ex);
            //    telemetryClient.Flush();
            //    Thread.Sleep(5000);
            //};
        }
    }
}
