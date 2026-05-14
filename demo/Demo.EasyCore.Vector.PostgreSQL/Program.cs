using Demo.Common.Agent;
using EasyCore.Agent;
using EasyCore.Dependencie;
using EasyCore.Vector.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<QianwenClientOptions>(builder.Configuration.GetSection(QianwenClientOptions.SectionName));
builder.Services.EasyCoreDependencie();
builder.Services.EasyCoreAgent(o => o.AgentContextStoreType = AgentContextStoreType.Memory);
builder.Services.EasyCorePostgreSql(o =>
{
    o.ConnectionString = builder.Configuration["PostgreSQL:ConnectionString"]
        ?? "Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=postgres;";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
