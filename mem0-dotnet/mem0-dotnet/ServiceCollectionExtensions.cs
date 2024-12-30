using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Qdrant.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mem0_dotnet
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMem0DotNet(this IServiceCollection services, Action<Mem0Options> value)
        {
            services.AddScoped<Mem0StoreService>();
            services.AddScoped<Mem0Service>();

            var options = new Mem0Options();
            value.Invoke(options);

            services.AddOptions<Mem0Options>().Configure(value);

            var kernelBuilder = services
               .AddDashScopeChatCompletion(options.Key, options.ChatCompletionModel).AddDashScopeTextEmbeddingGeneration(options.Key, options.TextEmbeddingModel).AddKernel();

            kernelBuilder.Services.AddScoped<MemoryKernelFunction>();
            kernelBuilder.Plugins.AddFromType<MemoryKernelFunction>();

            return services;
        }

        public static IServiceCollection AddMem0DotNetStore(this IServiceCollection services, Action<Mem0StoreOptions> value)
        {
            var options = new Mem0StoreOptions();
            value.Invoke(options);

            services.AddOptions<Mem0StoreOptions>().Configure(value);

            services.AddScoped<QdrantClient>((x) =>
            {
                var client = new QdrantClient(options.Host);

                return client;
            });

            return services;
        }
    }
}
