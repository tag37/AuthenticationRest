using System.Reflection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.Channel;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

// Setup TelemetryClient with channel
builder.Services.AddSingleton<TelemetryClient>(sp =>
{
    var config = new TelemetryConfiguration
    {
        ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"],
        TelemetryChannel = new InMemoryChannel()
    };

    return new TelemetryClient(config);
});

var host = builder.Build();

await host.StartAsync(); // ensure services are initialized

var telemetryClient = host.Services.GetRequiredService<TelemetryClient>();

try
{
    throw new InvalidOperationException("Something went wrong!");
}
catch (Exception ex)
{
    var assembly = Assembly.GetExecutingAssembly().GetName();
    telemetryClient.TrackTrace("📘 Test trace from TelemetryClient", SeverityLevel.Information);
    var exceptionTelemetry = new ExceptionTelemetry(ex)
    {
        SeverityLevel = SeverityLevel.Critical,
        HandledAt = ExceptionHandledAt.Unhandled
    };

    exceptionTelemetry.Properties["assembly"] = assembly.Name ?? "Unknown";
    exceptionTelemetry.Properties["version"] = assembly.Version?.ToString() ?? "Unknown";

    telemetryClient.TrackException(exceptionTelemetry);
    telemetryClient.Flush();

    await Task.Delay(3000); // Give time to send
}

await host.StopAsync();
