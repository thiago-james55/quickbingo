using QuickBingo.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<QuickBingo.Services.BingoService>();
var app = builder.Build();


app.MapGet("/", () => "Hello World!");
app.MapHub<BingoHub>("/bingoHub");

app.Run();
