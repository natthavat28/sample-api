using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Refit;

namespace sample_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHealthChecks();

            // Add services to the container.
            builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("sample-api"))
            .WithMetrics(builder =>
            {
                builder
                    // Monitor incoming HTTP requests (Controller actions)
                    .AddAspNetCoreInstrumentation()

                    // Monitor outgoing HTTP requests (HttpClient)
                    .AddHttpClientInstrumentation()

                    // Monitor SQL Database calls automatically
                    .AddSqlClientInstrumentation()

                    // Monitor Redis (requires passing your ConnectionMultiplexer)
                    // .AddRedisInstrumentation(connectionMultiplexer) 

                    // Export to Prometheus
                    .AddPrometheusExporter();
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            

            builder.Services.AddRefitClient<ISampleExternalApiCaller>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.restful-api.dev"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
