using Microsoft.AspNetCore.Server.Kestrel.Core;
using Msa.ServiceDefaults.OpenTelemetry;
using Msa.Services.ServiceB.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(x =>
{
    x.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc(x =>
{
    x.EnableDetailedErrors = true;
});

builder.Services.AddGrpcClient<Msa.Protos.Weather.WeatherForecastService.WeatherForecastServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcHost:ServiceC"]!);

});
builder.Services.AddOpenTelemetry(builder.Environment, builder.Configuration);
/*builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(x => x.AddOtlpExporter(x => x.Endpoint = new Uri("http://msa.otel-collector:4317")));*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseSerilogRequestLoggingWithHeaders(builder.Configuration);
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<ActorService>();

app.MapControllers();

app.Run();
