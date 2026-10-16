param(
    [string]$ListenUrl = "http://localhost:4318",
    [string]$LogFile = "otel-echo.log"
)

$ErrorActionPreference = "Stop"

# Ensure the output file exists.
New-Item -ItemType File -Path $LogFile -Force | Out-Null

$listener = [System.Net.HttpListener]::new()
$listener.Prefixes.Add($ListenUrl.TrimEnd('/') + "/")

try
{
    $listener.Start()
    Write-Host "OTEL echo server listening on $ListenUrl"
    Write-Host "Log file: $LogFile"

    $contextTask = $null
    while ($listener.IsListening)
    {
        # Reuse the pending task; starting a new one each pass orphans incoming requests.
        if ($null -eq $contextTask) {
            $contextTask = $listener.GetContextAsync()
        }
        if (-not $contextTask.IsCompleted) {
            Start-Sleep -Milliseconds 100
            continue
        }
        $context = $contextTask.GetAwaiter().GetResult()
        $contextTask = $null
        $request = $context.Request
        $response = $context.Response

        $bodyBytes = [System.IO.Stream]::Null
        try
        {
            $bodyBytes = $request.InputStream
            $reader = [System.IO.StreamReader]::new($bodyBytes, [System.Text.Encoding]::UTF8, $true)
            $rawBody = $reader.ReadToEnd()

            $timestamp = (Get-Date).ToString("o")
            $line = "[$timestamp] $($request.HttpMethod) $($request.Url.AbsoluteUri)"
            Add-Content -Path $LogFile -Value $line
            Add-Content -Path $LogFile -Value $rawBody
            Add-Content -Path $LogFile -Value ""

            Write-Host $rawBody
        }
        catch
        {
            $timestamp = (Get-Date).ToString("o")
            Add-Content -Path $LogFile -Value "[$timestamp] ERROR: $($_.Exception.Message)"
            Write-Error $_
        }
        finally
        {
            if ($null -ne $bodyBytes) { $bodyBytes.Dispose() }
        }

        $response.StatusCode = 200
        $response.ContentType = "application/json"
        $response.ContentLength64 = [System.Text.Encoding]::UTF8.GetByteCount('{"status":"ok"}')
        $buffer = [System.Text.Encoding]::UTF8.GetBytes('{"status":"ok"}')
        $response.OutputStream.Write($buffer, 0, $buffer.Length)
        $response.OutputStream.Close()
        $response.Close()
    }
}
finally
{
    if ($listener) {
        try { $listener.Stop() } catch {}
        try { $listener.Close() } catch {}
    }
    Write-Host "OTEL echo server stopped."
}
