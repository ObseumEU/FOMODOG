using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace FomoDog.OpenTelemetry
{
    public static class OpenTelemetry
    {
        public static readonly ActivitySource Source = new("fomodog");

        public static void AddConsoleOpenTelemetry(this IServiceCollection services, IConfigurationSection configSection)
        {
            var openTelemetryOptions = configSection.Get<OpenTelemetryOptions>();

            services.Configure<OpenTelemetryOptions>(configSection);
            Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
            .AddService("fomodog"))
           .AddSource("fomodog")
           .AddHttpClientInstrumentation()
           .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(openTelemetryOptions.UrlGrpc);
                options.Protocol = OtlpExportProtocol.Grpc;
            })
           .Build();
        }
    }
}
