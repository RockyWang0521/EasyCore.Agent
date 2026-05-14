using Demo.Common.Agent;
using EasyCore.Agent;
using EasyCore.Dependencie;
using EasyCore.Vector.Milvus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<QianwenClientOptions>(builder.Configuration.GetSection(QianwenClientOptions.SectionName));
builder.Services.EasyCoreDependencie();
builder.Services.EasyCoreAgent(o => o.AgentContextStoreType = AgentContextStoreType.Memory);
builder.Services.EasyCoreMilvus(o =>
{
    o.Host = builder.Configuration["Milvus:Host"] ?? "localhost";
    o.Port = int.Parse(builder.Configuration["Milvus:Port"] ?? "19530");
    o.DatabaseName = builder.Configuration["Milvus:DatabaseName"] ?? "default";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
