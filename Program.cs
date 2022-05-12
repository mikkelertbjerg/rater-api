using StackExchange.Redis;

ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect("localhost");
var redis = multiplexer.GetDatabase();

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
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
var app = builder.Build();
app.UseCors(localCorsPolicy);

app.MapPost("/session", async (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var session = new Session();
    var key = $"session:{session.Id}";
    var value = session.Id;
    await db.StringSetAsync(key, value);
    return Results.Created($"/session/{session.Id}", session);
});

app.MapGet("/session/{id:string}", async (string id, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var value = await db.StringGetAsync($"session:{id}");
    if (value.IsNullOrEmpty)
    {
        Results.NotFound();
    };
    Results.Ok(value);
});



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
