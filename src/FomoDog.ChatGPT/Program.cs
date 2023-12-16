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

void ConfigureResource(ResourceBuilder r)
{
    r.AddService("Service Name",
        serviceVersion: "Version",
        serviceInstanceId: Environment.MachineName);
}

IHostEnvironment env = Host.CreateDefaultBuilder(args).Build().Services.GetRequiredService<IHostEnvironment>();
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, true)
    .AddEnvironmentVariables()
    .Build();

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

         services.AddChatGPTChatClient(config);

     })
     .Build()
     .Run(); ;

