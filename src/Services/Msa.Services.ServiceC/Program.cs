using Msa.ServiceDefaults.OpenTelemetry;
using Msa.Services.ServiceC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddOpenTelemetry(builder.Environment, builder.Configuration);
/*builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(x => x.AddOtlpExporter(x => x.Endpoint = new Uri("http://msa.otel-collector:4317")));*/

var app = builder.Build();

app.UseSerilogRequestLoggingWithHeaders(builder.Configuration);
// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<WeatherForecastService>();


app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
