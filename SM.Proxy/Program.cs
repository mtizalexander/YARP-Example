using SM.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.AddTelemetry();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();

app.Run();