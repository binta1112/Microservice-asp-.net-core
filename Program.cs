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
        Console.WriteLine("redis******");
        Console.WriteLine(redisConnString);
        Console.WriteLine("Tentative de connexion...");
       // var redis = ConnectionMultiplexer.Connect("REDIS_CONNECTION_STRING");
        var redis = ConnectionMultiplexer.Connect(
            new ConfigurationOptions
            {
                EndPoints = { { "redis-19414.c256.us-east-1-2.ec2.cloud.redislabs.com", 19414 } },
                User = "default",
                Password = "PMBr7eOYP2quaiBLN2GeyQHTtsW3nbB2"
            }

            );
        redisDb = redis.GetDatabase();
        Console.WriteLine("Connecté a Redis !");

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Redis non pret, retry dans 1s... ({ex.Message})");
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
