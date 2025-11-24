using MicroServicePanier.Service;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// -------------------
// Connexion Redis avec retry
// -------------------
IDatabase redisDb = null!;
while (redisDb == null)
{
    try
    {
        var redisConnString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        var options = ConfigurationOptions.Parse(redisConnString);
        options.AbortOnConnectFail = false;
        Console.WriteLine("redis******",redisDb);
        Console.WriteLine("Tentative de connexion à Redis...");
        var redis = ConnectionMultiplexer.Connect("REDIS_CONNECTION_STRING");
        redisDb = redis.GetDatabase();
        Console.WriteLine("Connecté à Redis !");

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Redis non prêt, retry dans 1s... ({ex.Message})");
        Thread.Sleep(1000);
    }
}

// -------------------
// Injection des services
// -------------------
builder.Services.AddSingleton<IDatabase>(redisDb);
builder.Services.AddSingleton<PanierService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -------------------
// Middleware
// -------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Si tu ne veux pas HTTPS en local, tu peux commenter cette ligne
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// -------------------
// Démarrage
// -------------------
app.Run("http://0.0.0.0:8080"); // important pour Docker
