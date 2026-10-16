using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SM.Common.Configuration;

namespace SM.Proxy;

public static class TelemetryConfiguration
{
    public const string ServiceName = "StarMap";
    public static void AddTelemetry(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(x =>
        {
            x.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
                .AddOtlpExporter();
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(x => x.AddService(ServiceName))
            .WithTracing(t => t.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("Yarp.ReverseProxy")
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new HostSettings().OtlpEndpointUri;
                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                }));
    }
}