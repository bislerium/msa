namespace Msa.ServiceDefaults.OpenTelemetry
{
    internal class OtelOptions
    {
        internal const string SectionName = "Otel";
        internal string CollectorEndpoint { get; init; } = string.Empty;
        internal TraceConfig Trace { get; init; } = new TraceConfig();
        internal MetricConfig Metric { get; init; } = new MetricConfig()
        {
            Meters =
            [
/*                "Microsoft.AspNetCore.Hosting",
                "Microsoft.AspNetCore.Server.Kestrel",
                "Microsoft.AspNetCore.Http.Connections",
                "Microsoft.AspNetCore.Routing",
                "Microsoft.AspNetCore.Diagnostics",
                "Microsoft.AspNetCore.RateLimiting",
                "System.Net.NameResolution",
                "System.Net.Http"*/
            ]
        };
        internal LogConfig Log { get; init; } = new LogConfig()
        {
            ExportToConsole = true
        };
    }

    internal class TraceConfig
    {
        internal bool ExportToConsole { get; init; }
    }

    internal class MetricConfig
    {
        internal string[] Meters { get; init; } = [];
        internal bool ExportToConsole { get; init; }
    }

    internal class LogConfig
    {
        internal bool ExportToConsole { get; init; }
    }
}