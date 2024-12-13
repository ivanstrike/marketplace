using ProductCartMicroservice.Data;
using ProductCartMicroservice.Services;
using Serilog;
using UserMicroservice.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddSingleton<RabbitMqConsumer>();
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddDbContext<DbContextClass>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
var rabbitMqConsumer = app.Services.GetRequiredService<RabbitMqConsumer>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
