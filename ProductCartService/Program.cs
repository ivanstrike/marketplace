using ProductCartMicroservice.Data;
using ProductCartMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddSingleton<RabbitMqConsumer>();
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
var rabbitMqConsumer = app.Services.GetRequiredService<RabbitMqConsumer>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
