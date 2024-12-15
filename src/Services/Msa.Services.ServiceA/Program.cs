using Msa.ServiceDefaults.OpenTelemetry;
using Msa.ServiceDefaults.Redis;
using Msa.Services.ServiceA.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedis(builder.Configuration);
builder.Services.AddScoped<WeatherForecastService>();

var serviceBConnectionString = builder.Configuration.GetConnectionString("ServiceB");
ArgumentNullException.ThrowIfNullOrEmpty(serviceBConnectionString);

builder.Services.AddHttpClient("ServiceB", client =>
{


    client.BaseAddress = new Uri(serviceBConnectionString);
    client.DefaultRequestVersion = HttpVersion.Version20;
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;

});
builder.Services.AddGrpcClient<Msa.Protos.Actor.ActorService.ActorServiceClient>(o =>
{
    o.Address = new Uri(serviceBConnectionString);

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
