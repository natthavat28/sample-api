using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
                .WithTracing(tracing =>
                {
                    // capture incoming HTTP requests, spans for controllers/middleware
                    tracing.AddAspNetCoreInstrumentation(options =>
                    {
                        // EnrichWithHttpRequest is called with the HttpRequest when available.
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            var endpoint = request.HttpContext.GetEndpoint();
                            if (endpoint is RouteEndpoint routeEndpoint)
                            {
                                activity?.SetTag("http.route", routeEndpoint.RoutePattern.RawText);
                            }
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        // Monitor incoming HTTP requests (Controller actions)
                        .AddAspNetCoreInstrumentation()

                        // Monitor outgoing HTTP requests (HttpClient)
                        .AddHttpClientInstrumentation()

                        // Monitor SQL Database calls automatically
                        .AddSqlClientInstrumentation()

                        // Register custom meter so custom counters are exported
                        .AddMeter("sample-api-metrics")

                        // Export to Prometheus
                        .AddPrometheusExporter();
                });

            builder.Services.AddControllers();

            builder.Services.AddRefitClient<ISampleExternalApiCaller>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.restful-api.dev"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            // Ensure routing is set up before the Prometheus scrape endpoint and controller mapping.
            app.UseRouting();

            // Create a Meter and Counter for per-route request counts
            var meter = new System.Diagnostics.Metrics.Meter("sample-api-metrics");
            var requestCounter = meter.CreateCounter<long>("http_server_requests_total");

            // Middleware to increment counter per request (exclude metrics scrape path)
            app.Use(async (context, next) =>
            {
                // Execute the rest of the pipeline first to capture response status
                await next();

                // Do not count metrics scrapes
                if (context.Request.Path.StartsWithSegments("/metrics"))
                {
                    return;
                }

                var endpoint = context.GetEndpoint() as RouteEndpoint;
                var route = endpoint?.RoutePattern?.RawText ?? context.Request.Path.Value ?? "/";
                var status = context.Response?.StatusCode.ToString() ?? "0";

                requestCounter.Add(1, new KeyValuePair<string, object?>("http_route", route),
                                         new KeyValuePair<string, object?>("http_method", context.Request.Method),
                                         new KeyValuePair<string, object?>("http_status_code", status));
            });

            // Expose Prometheus /metrics endpoint (pull-based). Must come after UseRouting().
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
