using CommandService.AsyncDataServices;
using CommandService.Data;
using CommandService.EventProcessing;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program)); // AutoMapper DI
builder.Services.AddScoped<ICommandRepo, CommandRepo>(); // Repository DI
builder.Services.AddSingleton<IEventProcessor, EventProcessor>(); // Event Processor DI
builder.Services.AddHostedService<MessageBusSubscriber>(); // Message Bus Subscriber DI
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem")); // In-memory database DI

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
