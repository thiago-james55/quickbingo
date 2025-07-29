using QuickBingo.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<QuickBingo.Services.BingoService>();

var app = builder.Build();

app.UseCors(); // <--- essencial aqui

app.MapGet("/", () => "Hello World!");
app.MapHub<BingoHub>("/bingoHub");

app.Run();
