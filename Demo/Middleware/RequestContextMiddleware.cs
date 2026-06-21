using System.Diagnostics;

namespace Demo.Middleware;

public sealed class RequestContextMiddleware(ILogger<RequestContextMiddleware> logger) : IMiddleware
{
    private const string CorrelationHeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers[CorrelationHeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationHeaderName] = correlationId;

        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation("Handling {Method} {Path}", context.Request.Method, context.Request.Path);

        await next(context);

        stopwatch.Stop();
        logger.LogInformation(
            "Completed {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
