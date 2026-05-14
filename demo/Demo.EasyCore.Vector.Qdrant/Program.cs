using Demo.Common.Agent;
using EasyCore.Agent;
using EasyCore.Dependencie;
using EasyCore.Vector.Qdrant;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<QianwenClientOptions>(builder.Configuration.GetSection(QianwenClientOptions.SectionName));
builder.Services.EasyCoreDependencie();
builder.Services.EasyCoreAgent(o => o.AgentContextStoreType = AgentContextStoreType.Memory);
builder.Services.EasyCoreQdrant(o =>
{
    o.Host = builder.Configuration["Qdrant:Host"] ?? "localhost";
    o.GrpcPort = int.Parse(builder.Configuration["Qdrant:GrpcPort"] ?? "6334");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
