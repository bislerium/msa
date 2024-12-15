using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Msa.ServiceDefaults.OpenTelemetry
{
    public static class ServiceExtensions
    {
        public static void AddOpenTelemetry(this IServiceCollection services, IHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            //var otlp_collector_endpoint = "http://msa.otel-collector:4317";

            #region OTLP options from configuration

            var otelOptions = configuration
                .GetSection(OtelOptions.SectionName)
                .Get<OtelOptions>(options => { options.BindNonPublicProperties = true; })
                ?? new OtelOptions();

            #endregion


            #region Info

            #region Calling Assembly Info

            var callingAssembly = Assembly.GetCallingAssembly();
            var callingAssemblyName = callingAssembly.GetName();

            #endregion

            #region Service Info

            var serviceName = callingAssemblyName.Name ?? "unknown_service";
            var serviceNameSpace = callingAssemblyName?.Name?.Split('.')[0] ?? "unknown_service_namespace";
            var serviceVersion = callingAssemblyName?.Version?.ToString() ?? "unknown_version";
            var serviceInformationalVersion = callingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown_informational_version";

            #endregion

            #region Environment Info 

            var environmentName = hostEnvironment.EnvironmentName;
            string environmentUserName = ((!string.IsNullOrWhiteSpace(Environment.UserDomainName))
                ? (Environment.UserDomainName + "\\" + Environment.UserName)
                : Environment.UserName);

            #endregion

            #region Runtime Info

            var runtimeName = ".NET";
            var runtimeVersion = Environment.Version.ToString();
            var runtimeDescription = RuntimeInformation.FrameworkDescription;

            #endregion

            #region Host Info

            var hostName = Environment.MachineName;
            var hostArchitecture = RuntimeInformation.ProcessArchitecture.ToString();

            #endregion

            #region OS Info

            var osType = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "windows"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "linux"
                    : "unknown";

            var osArchitecture = RuntimeInformation.OSArchitecture.ToString();
            var osVersion = Environment.OSVersion.ToString();
            var osDescription = RuntimeInformation.OSDescription;
            var osRuntimeId = RuntimeInformation.RuntimeIdentifier;

            #endregion

            #region Static Attributes

            var attributes = new Dictionary<string, object>
            {
                ["service.informational_version"] = serviceInformationalVersion,
                ["deployment.environment"] = environmentName,
                ["deployment.environment_username"] = environmentUserName,
                ["host.name"] = hostName,
                ["host.arch"] = hostArchitecture,
                ["os.type"] = osType,
                ["os.arch"] = osArchitecture,
                ["os.version"] = osVersion,
                ["os.description"] = osDescription,
                ["os.runtime_id"] = osRuntimeId,
                ["runtime.name"] = runtimeName,
                ["runtime.version"] = runtimeVersion,
                ["runtime.description"] = runtimeDescription,
            };

            #endregion

            #endregion

            #region Add OpenTelemetry for Tracing and Metrics            

            if (!string.IsNullOrEmpty(otelOptions.CollectorEndpoint))
            {
                services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder => resourceBuilder.AddService
                (
                    serviceName: serviceName,
                    serviceNamespace: serviceNameSpace,
                    serviceVersion: serviceVersion
                )
                .AddAttributes(attributes))
                .WithTracing(x =>
                {
                    x.AddAspNetCoreInstrumentation()
                        .AddRedisInstrumentation()
                        .AddGrpcClientInstrumentation()
                        .AddRedisInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddOtlpExporter(x =>
                         {
                             x.ExportProcessorType = ExportProcessorType.Batch;
                             x.Protocol = OtlpExportProtocol.Grpc;
                             x.Endpoint = new Uri(otelOptions.CollectorEndpoint);
                         });

                    if (otelOptions.Trace.ExportToConsole)
                    {
                        x.AddConsoleExporter();
                    }
                })
                .WithMetrics(x =>
                {
                    x
                    .AddMeter(otelOptions.Metric.Meters)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddOtlpExporter(x =>
                        {
                            x.ExportProcessorType = ExportProcessorType.Batch;
                            x.Protocol = OtlpExportProtocol.Grpc;
                            x.Endpoint = new Uri(otelOptions.CollectorEndpoint);
                        });

                    if (otelOptions.Metric.ExportToConsole)
                    {
                        x.AddConsoleExporter();
                    }
                });
            }

            #endregion

            #region Add Serilog for Logging

            // Log.Logger 

            var loggerConfiguration = new LoggerConfiguration()
                 .Enrich.WithProcessId()
                 .Enrich.WithProcessName()
                 .Enrich.WithThreadId()
                 .Enrich.WithThreadName()
                 .Enrich.WithMemoryUsage()
                 .Enrich.FromLogContext();

            if (!string.IsNullOrEmpty(otelOptions.CollectorEndpoint))
            {
                loggerConfiguration
                    .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otelOptions.CollectorEndpoint;
                    options.Protocol = OtlpProtocol.Grpc;
                    options.OnBeginSuppressInstrumentation = SuppressInstrumentationScope.Begin;
                    options.ResourceAttributes = new Dictionary<string, object>(attributes)
                    {
                        ["service.name"] = serviceName,
                        ["service.namespace"] = serviceNameSpace,
                        ["service.version"] = serviceVersion,
                        ["service.informational_version"] = serviceInformationalVersion
                    };
                });
            }

            if (otelOptions.Log.ExportToConsole)
            {
                loggerConfiguration
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    .WriteTo.Debug();
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            services.AddSerilog();

            #endregion

        }

        public static void UseSerilogRequestLoggingWithHeaders(this IApplicationBuilder builder, IConfiguration configuration)
        {

            var blackListedHeaders = configuration.GetSection("Otel:Log:BlackListedHeaders").Get<string[]>() ?? [];

            builder.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    foreach (var header in httpContext.Request.Headers.ExceptBy(blackListedHeaders, y => y.Key))
                    {
                        diagnosticContext.Set($"Request_Header_{header.Key}", header.Value.ToString());
                    }

                    foreach (var header in httpContext.Response.Headers.ExceptBy(blackListedHeaders, y => y.Key))
                    {
                        diagnosticContext.Set($"Response_Header_{header.Key}", header.Value.ToString());
                    }
                };
            });
        }
    }
}
