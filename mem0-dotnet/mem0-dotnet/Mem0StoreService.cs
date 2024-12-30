using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mem0_dotnet
{
    public class Mem0StoreService
    {
        private QdrantClient _qdrantClient;
        private readonly Mem0Options _mem0Options;
        private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
        public Mem0StoreService(QdrantClient qdrantClient, IOptions<Mem0Options> mem0Options, ITextEmbeddingGenerationService textEmbeddingGenerationService)
        {
            _textEmbeddingGenerationService = textEmbeddingGenerationService;
            _qdrantClient = qdrantClient;
            _mem0Options = mem0Options.Value;
        }

        internal async Task AddMemoryAsync(string data, string userId)
        {
            var embeddings = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(data);

            var point = new PointStruct()
            {
                Id = Guid.NewGuid(),
                Vectors = embeddings.ToArray()
            };

            point.Payload.Add("data", data);
            point.Payload.Add("userId", userId);
            point.Payload.Add("updatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            await _qdrantClient.UpsertAsync(_mem0Options.Collection, new List<PointStruct> { point });
        }

        internal async Task UpdateMemoryAsync(string memoryId, string data, string userId)
        {
            var embeddings = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(data);

            var point = new PointStruct()
            {
                Id = Guid.Parse(memoryId),
                Vectors = embeddings.ToArray()
            };

            point.Payload.Add("data", data);
            point.Payload.Add("userId", userId);
            point.Payload.Add("updatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            await _qdrantClient.UpsertAsync(_mem0Options.Collection, new List<PointStruct> { point });

        }

        internal async Task<UpdateResult> DeleteMemory(string userId, string memoryId)
        {
            var result = await _qdrantClient.DeleteAsync(_mem0Options.Collection, Guid.Parse(memoryId));

            return result;
        }
    }
}
