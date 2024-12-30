using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mem0_dotnet
{
    public class Mem0Options
    {
        public string Key { get; set; }
        public string ChatCompletionModel { get; set; }
        public string TextEmbeddingModel { get; set; }
        public string Collection { get; set; }
        public uint Limit { get; set; }
    }
}
