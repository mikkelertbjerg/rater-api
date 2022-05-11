using StackExchange.Redis;

var localCorsPolicy = "_local";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy(localCorsPolicy, policy =>
    {
        policy.AllowAnyOrigin() //WithOrigins("http://localhost:3000/")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
var app = builder.Build();
app.UseCors(localCorsPolicy);

ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
var db = redis.GetDatabase();

app.MapPost("/session", () => new Session());

app.Run();

public class Session
{
    private static readonly Random random = new();
    private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Id { get; }
    public Session()
    {
        Id = new string(Enumerable.Repeat(_chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
