using Cnblogs.SemanticKernel.Connectors.DashScope;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace mem0_dotnet
{
    public class Mem0Service
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly QdrantClient _qdrantClient;
        private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
        private readonly Mem0Options _mem0Options;

        private const string MemoryDeductionPrompt = """
                                                 Deduce the facts, preferences, and memories from the provided text.
                                                 Just return the facts, preferences, and memories in bullet points:
                                                 Natural language text: {user_input}
                                                 UserId: {user_id}

                                                 Constraint for deducing facts, preferences, and memories:
                                                 - The facts, preferences, and memories should be concise and informative.
                                                 - Don't start by "The person likes Pizza". Instead, start with "Likes Pizza".
                                                 - Don't remember the user/agent details provided. Only remember the facts, preferences, and memories.
                                                 - Output content in the original language

                                                 Deduced facts, preferences, and memories:
                                                 """;

        private const string UpdateMemoryPrompt = """
                                              You are an expert at merging, updating, and organizing memories. When provided with existing memories and new information, your task is to merge and update the memory list to reflect the most accurate and current information. You are also provided with the matching score for each existing memory to the new information. Make sure to leverage this information to make informed decisions about which memories to update or merge.

                                              Guidelines:
                                              - Eliminate duplicate memories and merge related memories to ensure a concise and updated list.
                                              - If a memory is directly contradicted by new information, critically evaluate both pieces of information:
                                                  - If the new memory provides a more recent or accurate update, replace the old memory with new one.
                                                  - If the new memory seems inaccurate or less detailed, retain the original and discard the old one.
                                              - Maintain a consistent and clear style throughout all memories, ensuring each entry is concise yet informative.
                                              - If the new memory is a variation or extension of an existing memory, update the existing memory to reflect the new information.

                                              Here are the details of the task:
                                              - UserId: {user_id}

                                              - Existing Memories:
                                              {existing_memories}

                                              - New Memory: {memory}
                                              """;

        public Mem0Service(QdrantClient qdrantClient, ITextEmbeddingGenerationService textEmbeddingGenerationService, IOptions<Mem0Options> mem0Options, IChatCompletionService chatCompletionService, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _chatCompletionService = chatCompletionService;
            _qdrantClient = qdrantClient;
            _textEmbeddingGenerationService = textEmbeddingGenerationService;
            _mem0Options = mem0Options.Value;
        }

        public async Task<string> SaveMemory(string userId, string data)
        {
            var embeddings = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(data);

            var filters = new Dictionary<string, object>() { { "userId", userId } };
            var filter = CreateFilter(filters);

            var existing_memories = (await _qdrantClient.SearchAsync(_mem0Options.Collection, embeddings.ToArray(), filter, limit: _mem0Options.Limit, vectorsSelector: new WithVectorsSelector
            {
                Enable = true
            })).Select(x => new
            {
                memoryId = Guid.Parse(x.Id.Uuid),
                score = x.Score,
                text = x.Payload.ToDictionary(x => x.Key, x => x.Value.StringValue)["data"]
            });

            var chatHistory = new ChatHistory();

            string prompt = MemoryDeductionPrompt.Replace("{user_input}", data)
               .Replace("{user_id}", userId);

            var extracted_memories = await _chatCompletionService.GetChatMessageContentAsync(new ChatHistory()
            {
                new(AuthorRole.System,
                    "You are an expert at deducing facts, preferences and memories from unstructured text."),
                new(AuthorRole.User, prompt)
            });

            var messages = new List<ChatMessageContent>()
            {
                new(AuthorRole.User,
                    UpdateMemoryPrompt.Replace("{user_id}",userId).Replace("{existing_memories}",
                            JsonSerializer.Serialize(existing_memories, new JsonSerializerOptions()
                            {
                                // utf8 encoding
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            }))
                        .Replace("{memory}", extracted_memories.Content))
            };

            chatHistory.AddRange(messages);

            using var scope = _serviceProvider.CreateScope();
            var kernel = scope.ServiceProvider.GetService<Kernel>();

            var content = await _chatCompletionService.GetChatMessageContentAsync(chatHistory,
               new DashScopePromptExecutionSettings()
               {
                   ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
               }, kernel);

            return content.Content;
        }


        public async Task<IReadOnlyList<ScoredPoint>> SearchMemory(string userId, string query)
        {
            var embeddings = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(query);

            var filters = new Dictionary<string, object>() { { "userId", userId } };
            var filter = CreateFilter(filters);

            var existing_memories = await _qdrantClient.SearchAsync(_mem0Options.Collection, embeddings, filter, limit: _mem0Options.Limit, vectorsSelector: new WithVectorsSelector
            {
                Enable = true
            });

            return existing_memories;
        }

        public async Task<ScrollResponse> SearchMemory(string userId)
        {
            var filters = new Dictionary<string, object>() { { "userId", userId } };
            var filter = CreateFilter(filters);

            var existing_memories = await _qdrantClient.ScrollAsync(_mem0Options.Collection, filter, limit: _mem0Options.Limit, vectorsSelector: new WithVectorsSelector()
            {
                Enable = true,
            });

            return existing_memories;
        }
        public async Task<UpdateResult> DeleteMemory(string userId)
        {
            var filters = new Dictionary<string, object>() { { "userId", userId } };
            var filter = CreateFilter(filters);

            var result = await _qdrantClient.DeleteAsync(_mem0Options.Collection, filter);

            return result;
        }

        private Filter CreateFilter(Dictionary<string, object>? filters)
        {
            var conditions = new List<Condition>();

            foreach (var filter in filters)
            {
                conditions.Add(new Condition()
                {
                    Field = new FieldCondition
                    {
                        Key = filter.Key,
                        Match = new Match
                        {
                            Text = filter.Value.ToString()
                        }
                    }
                });
            }

            var filterValue = new Filter()
            {
                Must =
            {
                conditions
            }
            };
            return conditions.Any() ? filterValue : null;
        }

    }
}
