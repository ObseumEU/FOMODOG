using FomoDog.ChatGPT;
using MassTransit;
using MassTransit.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;



void ConfigureResource(ResourceBuilder r)
{
    r.AddService(nameof(FomoDog.ChatGPT),
        serviceInstanceId: Environment.MachineName);
}

IHostEnvironment env = Host.CreateDefaultBuilder(args).Build().Services.GetRequiredService<IHostEnvironment>();
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, true)
    .AddEnvironmentVariables()
    .Build();


var openTelemetryOptions = config.GetSection("OpenTelemetryOptions").Get<OpenTelemetryOptions>();
Sdk.CreateTracerProviderBuilder()
    .ConfigureResource(ConfigureResource)
    .AddSource(DiagnosticHeaders.DefaultListenerName)
    .AddConsoleExporter()
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(openTelemetryOptions.UrlGrpc);
        options.Protocol = OtlpExportProtocol.Grpc;
    })
    .Build();

Host.CreateDefaultBuilder(args)
     .ConfigureServices((hostContext, services) =>
     {
         services.AddMassTransit(x =>
         {
             x.AddConsumer<MessageConsumer>();
             x.UsingRabbitMq((context, cfg) =>
             {
                 var rabbitMQConfig = context.GetRequiredService<IConfiguration>().GetSection("RabbitMQ");
                 cfg.Host(rabbitMQConfig["Host"], rabbitMQConfig["VirtualHost"], h =>
                 {
                     h.Username(rabbitMQConfig["Username"]);
                     h.Password(rabbitMQConfig["Password"]);
                 });

                 cfg.ConfigureEndpoints(context);
             });
         });

         services.AddChatGPTChatClient(config);

     })
     .Build()
     .Run();