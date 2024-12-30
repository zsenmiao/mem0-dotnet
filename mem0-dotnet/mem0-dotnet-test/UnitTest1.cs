using mem0_dotnet;
using Microsoft.Extensions.DependencyInjection;

namespace mem0_dotnet_test
{
    public class UnitTest1
    {
        private ServiceProvider service;

        public UnitTest1()
        {
            var services = new ServiceCollection();

            services.AddMem0DotNet(x =>
            {
                x.Key = "";
                x.ChatCompletionModel = "qwen-max";
                x.TextEmbeddingModel = "text-embedding-v3";
                x.Collection = "mem0-test2";
                x.Limit = 1;
            });

            services.AddMem0DotNetStore(x =>
            {
                x.Host = "localhost";
            });

            service = services.BuildServiceProvider();
        }

        [Fact]
        public async Task SearchMemory()
        {
            try
            {
                var mem0 = service.GetService<Mem0Service>();

                var result = await mem0.SearchMemory("123");
            }
            catch (Exception e)
            {

                throw;
            }
        }

        [Fact]
        public async Task SearchMemory2()
        {
            try
            {
                var mem0 = service.GetService<Mem0Service>();

                var result = await mem0.SearchMemory("123", "������������Ҫ��ʲô���ţ�");

               
            }
            catch (Exception e)
            {

                throw;
            }
        }

        [Fact]
        public async Task DeleteMemory()
        {
            try
            {
                var mem0 = service.GetService<Mem0Service>();

                var result = await mem0.DeleteMemory("123");
            }
            catch (Exception e)
            {

                throw;
            }
        }

        [Fact]
        public async Task SaveMemory()
        {
            try
            {
                var mem0 = service.GetService<Mem0Service>();

                await mem0.SaveMemory("123", "�������������һ��ȥ����Ӱ");
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}