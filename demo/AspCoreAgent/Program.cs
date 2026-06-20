using AspCoreAgent.Agent;
using EasyCore.Agent;
using EasyCore.Dependencie;
using EasyCore.Vector.Qdrant;
using EasyCore.Vector.PostgreSQL;
using EasyCore.Vector.Milvus;
using EasyCore.Vector.Redis;
using EasyCore.Vector.Elasticsearch;
using EasyCore.Workflow;

namespace AspCoreAgent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<DeepSeekClientOptions>(builder.Configuration.GetSection(DeepSeekClientOptions.SectionName));
            builder.Services.Configure<QianwenClientOptions>(builder.Configuration.GetSection(QianwenClientOptions.SectionName));

            builder.Services.EasyCoreDependencie();

            builder.Services.EasyCoreAgent();

            builder.Services.EasyCoreMilvus(options =>
            {
                options.Host = "localhost";
                options.Port = 19530;
                options.DatabaseName = "default";
                options.UserName = "";
                options.Password = "";
                options.UseTls = false;
            });

            builder.Services.EasyCoreQdrant(options =>
            {
                options.Host = "localhost";
                options.GrpcPort = 6334;
            });

            builder.Services.EasyCorePostgreSql(options =>
            {
                options.ConnectionString = "Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=Q123456;";
            });

            builder.Services.EasyCoreRedis(options =>
            {
                options.ConnectionString = "localhost:6379";
            });

            builder.Services.EasyCoreElasticsearch(options =>
            {
                options.Url = "http://localhost:9200";
            });

            builder.Services.EasyCoreWorkflow(options =>
            {
                options.StateStoreType = WorkflowStateStoreType.Memory;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
