using AspCoreAgent.Agent;
using EasyCore.Agent;
using EasyCore.Dependencie;

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
            builder.Services.EasyCoreDependencie();

            builder.Services.EasyCoreAgent(options =>
            {
                //options.MaxContextCount = 20;
                //options.AgentContextStoreType = AgentContextStoreType.Redis;
                //options.EndPoints = new List<string> { "127.0.0.1:6379" };
                //options.ConnectTimeout = 100;
                //options.SyncTimeout = 100;
                //options.DistributedName = "Web.EasyCore.Cache";
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
