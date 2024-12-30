using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mem0_dotnet
{
    public class MemoryKernelFunction
    {
        private readonly IServiceProvider _serviceProvider;

        public MemoryKernelFunction(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [KernelFunction, Description("add a memory")]
        public async Task AddMemory([Required][Description("data to add to memory")] string data, [Description("userId")] string userId)
        {
            using var scope = _serviceProvider.CreateScope();

            var store = scope.ServiceProvider.GetService<Mem0StoreService>();

            await store.AddMemoryAsync(data, userId);
        }

        [KernelFunction, Description("update a memory")]
        public async Task UpdateMemory([Required][Description("memoryId of the memory to update")] string memoryId,
            [Required] [Description("updated data for the memory")]
        string data, [Description("userId")] string userId)
        {
            using var scope = _serviceProvider.CreateScope();

            var store = scope.ServiceProvider.GetService<Mem0StoreService>();

            await store.UpdateMemoryAsync(memoryId, data, userId);
        }


        [KernelFunction, Description("delete memory by memoryId")]
        public async Task DeleteMemory([Required][Description("memoryId of the memory to update")] string memoryId, [Description("userId")] string userId)
        {
            using var scope = _serviceProvider.CreateScope();

            var store = scope.ServiceProvider.GetService<Mem0StoreService>();

            await store.DeleteMemory(userId,memoryId);
            
        }

    }
}
