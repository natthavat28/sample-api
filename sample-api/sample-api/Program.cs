using OpenTelemetry.Metrics;
using Refit;

namespace sample_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddOpenTelemetry()
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddRefitClient<ISampleExternalApiCaller>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.restful-api.dev"));

            builder.Services.AddHealthChecks();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
