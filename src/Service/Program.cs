var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var info = app.Configuration.GetSection("Service").Get<ServiceInfo>() ?? new ServiceInfo();
var sent = new List<Notification>();
var channels = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "email", "webhook" };

app.MapGet("/", () => Results.Ok(new { service = info.Name }));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapGet("/version", () => Results.Ok(new {
    service = info.Name,
    version = info.Version,
    commitSha = info.CommitSha,
    buildTime = info.BuildTime,
    environment = app.Environment.EnvironmentName,
}));
app.MapGet("/notifications", () => {
    lock (sent) {
        return sent.ToArray();
    }
});
app.MapPost("/notifications", (NotificationRequest request) => {
    if (!channels.Contains(request.Channel) || string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Body)) {
        return Results.Problem(title: "Channel, recipient, and body are required.", statusCode: StatusCodes.Status400BadRequest);
    }

    var notification = new Notification(Guid.CreateVersion7(), request.Channel, request.To.Trim(), request.Subject ?? "", request.Body, DateTimeOffset.UtcNow);
    lock (sent) {
        sent.Add(notification);
    }

    app.Logger.LogInformation("Sent {Channel} notification to {To}", notification.Channel, notification.To);
    return Results.Created($"/notifications/{notification.Id}", notification);
});

app.Run();

public sealed record NotificationRequest(string Channel, string To, string? Subject, string Body);
public sealed record Notification(Guid Id, string Channel, string To, string Subject, string Body, DateTimeOffset SentAt);

public sealed class ServiceInfo {
    public string Name { get; set; } = "notification-service";
    public string Version { get; set; } = "v0.0.0-dev";
    public string CommitSha { get; set; } = "local";
    public string BuildTime { get; set; } = "unknown";
}

public partial class Program;
