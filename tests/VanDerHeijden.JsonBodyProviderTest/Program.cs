using VanDerHeijden.JsonBodyProvider;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .Services
    .AddJsonBodyProvider(CorrectLists: true);

var app = builder.Build();

app.MapControllers();
app.Run();
