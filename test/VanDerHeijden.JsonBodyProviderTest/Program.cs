using VanDerHeijden.JsonBodyProvider;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddJsonBodyProvider();

var app = builder.Build();

app.MapControllers();
app.Run();
