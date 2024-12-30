using mem0_dotnet;

namespace mem0_dotnet_api_test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string Cros = "cros";
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: Cros,
                                  x =>
                                  {
                                      x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                                  });
            });

            builder.Services.AddMem0DotNet(x =>
            {
                x.Key = "";
                x.ChatCompletionModel = "qwen-plus";
                x.TextEmbeddingModel = "text-embedding-v3";
                x.Collection = "mem0-test2";
                x.Limit = 1;
            });

            builder.Services.AddMem0DotNetStore(x =>
            {
                x.Host = "localhost";
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseCors(Cros);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
