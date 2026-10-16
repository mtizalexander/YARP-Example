using System.Diagnostics;
using System.Net.Sockets;

namespace SM.Proxy.Tests;

[TestClass]
public sealed class YarpProxyIntegrationTests
{
    private static readonly string RepositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private static readonly Uri ProxyBaseAddress = new("http://localhost:8080");
    private static readonly List<(string name, int port, string projectPath)> ProjectDefinitions =
    [
        ("SM.Home", 8084, Path.Combine(RepositoryRoot, "Sites", "SM.Home", "SM.Home.csproj")),
        ("SM.Kepler90", 8081, Path.Combine(RepositoryRoot, "Sites", "SM.Kepler90", "SM.Kepler90.csproj")),
        ("SM.ProximaCentauri", 8082, Path.Combine(RepositoryRoot, "Sites", "SM.ProximaCentauri", "SM.ProximaCentauri.csproj")),
        ("SM.TRAPPIST1", 8083, Path.Combine(RepositoryRoot, "Sites", "SM.TRAPPIST1", "SM.TRAPPIST1.csproj")),
        ("SM.Proxy", 8080, Path.Combine(RepositoryRoot, "SM.Proxy", "SM.Proxy.csproj"))
    ];

    private static readonly List<Process> StartedProcesses = [];
    private static StarMapHttpClient _client = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        foreach (var project in ProjectDefinitions)
        {
            StartIfNotRunning(project.name, project.port, project.projectPath);
        }

        WaitForHost(ProxyBaseAddress);
        _client = new StarMapHttpClient(ProxyBaseAddress.ToString());
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        foreach (var process in StartedProcesses)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }

            process.Dispose();
        }

        StartedProcesses.Clear();
        _client.Dispose();
    }

    [TestMethod]
    public async Task Root_ProxiedHomePage_ReturnsHomeContent()
    {
        var response = await _client.GetHomeAsync();

        Assert.AreEqual(200, response.StatusCode);
        Assert.IsTrue(response.IsSuccessStatusCode);
        StringAssert.Contains(response.Body, "Choose Your Star");
        Assert.IsTrue(response.ExtractedTitles.Contains("Kepler-90"));
        Assert.IsTrue(response.ExtractedTitles.Contains("Proxima Centauri"));
        Assert.IsTrue(response.ExtractedTitles.Contains("TRAPPIST-1"));
    }

    [TestMethod]
    public async Task Kepler90Route_ProxiedToKepler90Site()
    {
        var response = await _client.GetKepler90Async();

        Assert.AreEqual(200, response.StatusCode);
        StringAssert.Contains(response.Body, "Kepler-90");
        Assert.IsTrue(response.ExtractedTitles.Contains("Kepler-90h"));
        Assert.IsTrue(response.ExtractedTitles.Contains("The Orrery Rail"));
    }

    [TestMethod]
    public async Task ProximaCentauriRoute_ProxiedToProximaCentauriSite()
    {
        var response = await _client.GetProximaCentauriAsync();

        Assert.AreEqual(200, response.StatusCode);
        StringAssert.Contains(response.Body, "Proxima Centauri");
        Assert.IsTrue(response.ExtractedTitles.Contains("Proxima b"));
        Assert.IsTrue(response.ExtractedTitles.Contains("Flare Watch"));
    }

    [TestMethod]
    public async Task Trappist1Route_ProxiedToTrappist1Site()
    {
        var response = await _client.GetTrappist1Async();

        Assert.AreEqual(200, response.StatusCode);
        StringAssert.Contains(response.Body, "TRAPPIST-1");
        Assert.IsTrue(response.ExtractedTitles.Contains("TRAPPIST-1e"));
        Assert.IsTrue(response.ExtractedTitles.Contains("Seven Worlds Skyway"));
    }

    [TestMethod]
    public async Task PrivacyPage_IsReachableThroughProxy()
    {
        var response = await _client.GetPrivacyAsync();

        Assert.AreEqual(200, response.StatusCode);
        StringAssert.Contains(response.Body, "Privacy");
    }

    private static void StartIfNotRunning(string name, int port, string projectPath)
    {
        if (IsPortListening(port))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\" --urls http://localhost:{port}",
            WorkingDirectory = RepositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        var process = new Process { StartInfo = startInfo };
        process.Start();
        StartedProcesses.Add(process);
    }

    private static void WaitForHost(Uri address)
    {
        var timeout = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < timeout)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                using var response = client.GetAsync(address).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Retry until the app is ready.
            }

            Thread.Sleep(500);
        }

        throw new TimeoutException($"The application at {address} did not become ready in time.");
    }

    private static bool IsPortListening(int port)
    {
        try
        {
            using var client = new TcpClient();
            client.Connect("localhost", port);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
