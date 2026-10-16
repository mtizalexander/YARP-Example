# StarMap

<img width="1489" height="465" alt="image" src="https://github.com/user-attachments/assets/01447d69-72d2-45c4-b424-b1e930883423" />


A demonstration of a simple [YARP](https://microsoft.github.io/reverse-proxy/) (Yet Another Reverse Proxy) configuration that reverse proxies requests between multiple ASP.NET Core sites.

## Features

- YARP reverse proxy (`SM.Proxy`) configured entirely through `appsettings.json` routes and clusters
- Path-based routing to separate site apps, with a catch-all route to the home site
- Four standalone ASP.NET Core MVC sites: Home, Kepler-90, Proxima Centauri, and TRAPPIST-1
- Shared libraries (`Common`, `Core`, `Domain`, `Infrastructure`) under `Shared/`
- OpenTelemetry tracing of ASP.NET Core, `HttpClient`, and YARP, exported over OTLP/HTTP
- Local PowerShell OTLP echo server for viewing telemetry
- End-to-end integration tests that send real HTTP requests through the proxy
- CodeTour walkthroughs in `.vscode/tours`

## Layout

| Path | Description |
| --- | --- |
| `SM.Proxy` | YARP reverse proxy |
| `Sites/` | Home, Kepler90, ProximaCentauri, TRAPPIST1 sites |
| `Shared/` | Shared libraries |
| `Tests/SM.Proxy.Tests` | Proxy integration tests |
| `Tools/Start-OtelServer.ps1` | Local OTLP receiver on port 4318 |

## Routing

| Route | Target |
| --- | --- |
| `/kepler90/{**remainder}` | http://localhost:8081 |
| `/proximaCentauri/{**remainder}` | http://localhost:8082 |
| `/trappist1/{**remainder}` | http://localhost:8083 |
| `/{**catch-all}` | http://localhost:8084 (Home) |

The proxy listens on http://localhost:8080.

## Prerequisites

- .NET SDK
- PowerShell (`pwsh`), optional, for the telemetry echo server

## Running

1. Build the solution:

   ```bash
   dotnet build StarMap.slnx
   ```

2. (Optional) Start the telemetry receiver:

   ```bash
   cd Tools
   pwsh ./Start-OtelServer.ps1
   ```

3. Run the "Debug All" compound launch profile

4. Browse to http://localhost:8080 if a browser doesn't automatically open.

5. Browse around the site and notice the routes in the url.

## Testing

```bash
dotnet test StarMap.slnx
```

The integration tests start the sites and proxy (skipping ports already in use) and verify requests reach the correct backend.

## Code tours

Install the [CodeTour](https://marketplace.visualstudio.com/items?itemName=vsls-contrib.codetour) VS Code extension and open the "Project Structure" and "YARP for IIS Users" tours.
