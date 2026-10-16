namespace SM.Common.Configuration;

public class HostSettings
{
    public string RootHost { get; set; } = "http://localhost:8080";
    public Uri OtlpEndpointUri => new Uri("http://localhost:4318");
}