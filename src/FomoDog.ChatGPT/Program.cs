using MassTransit;
using FomoDog.ChatGPT;
using MassTransit.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Trace;

Sdk.CreateTracerProviderBuilder()
    .ConfigureResource(ConfigureResource)
    .AddSource(DiagnosticHeaders.DefaultListenerName)
    .AddConsoleExporter()
    .Build();

CreateHostBuilder(args).Build().Run();

void ConfigureResource(ResourceBuilder r)
{
    r.AddService("Service Name",
        serviceVersion: "Version",
        serviceInstanceId: Environment.MachineName);
}


IHostBuilder CreateHostBuilder(string[] args) =>
 Host.CreateDefaultBuilder(args)
     .ConfigureServices((hostContext, services) =>
     {
         services.AddMassTransit(x =>
         {
             x.AddConsumer<MessageConsumer>();
             x.UsingRabbitMq((context, cfg) =>
             {
                 cfg.Host("localhost", "/", h =>
                 {
                     h.Username("guest");
                     h.Password("guest");
                 });

                 cfg.ConfigureEndpoints(context);
             });
         });

     });

