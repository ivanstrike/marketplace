using UserMicroservice.Data;
using UserMicroservice.Services;
using Serilog;
using UserMicroservice.Middleware;
using UserMicroservice.RabbitMQ;
using UserMicroservice.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddDbContext<DbContextClass>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<RabbitMqConsumer>();
builder.Services.AddHostedService<RabbitMqConsumerBackgroundService>();

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
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
