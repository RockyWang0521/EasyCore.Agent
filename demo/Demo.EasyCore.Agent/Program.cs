using Demo.Common.Agent;
using EasyCore.Agent;
using EasyCore.Dependencie;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DeepSeekClientOptions>(builder.Configuration.GetSection(DeepSeekClientOptions.SectionName));
builder.Services.Configure<QianwenClientOptions>(builder.Configuration.GetSection(QianwenClientOptions.SectionName));
builder.Services.EasyCoreDependencie();
builder.Services.EasyCoreAgent(options =>
{
    options.AgentContextStoreType = AgentContextStoreType.Memory;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
