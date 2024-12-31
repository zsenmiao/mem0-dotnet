## 介绍
本项目旨在演示如何使用 `mem0_dotnet` 库赋予大模型长期记忆的功能。

### 环境要求
- .NET Core SDK 或更新版本
- Visual Studio 或其他支持 .NET 的 IDE

### 安装步骤
1. 克隆或下载本项目到本地计算机。
2. 使用命令行工具或IDE打开解决方案文件（`.sln`）。
3. 恢复NuGet包：在命令行中运行 `dotnet restore` 或通过IDE完成此操作。
4. 根据需要配置 `mem0_dotnet` 和存储库设置（见下文配置部分）。

## 配置
在项目的构造函数中，我们设置了 `mem0_dotnet` 和 `mem0_dotnet_store` 的服务。你需要根据你的实际环境修改以下配置项：
- `Key`: 你的 通义千问 API密钥。
- `ChatCompletionModel`, `TextEmbeddingModel`: 根据需求选择合适的模型。
- `Collection`: 数据集名称。
- `Limit`: 查询结果的数量限制。
- `Host`: 存储服务的主机地址。

```csharp
services.AddMem0DotNet(x =>
{
    x.Key = "your_api_key_here";
    x.ChatCompletionModel = "qwen-max";
    x.TextEmbeddingModel = "text-embedding-v3";
    x.Collection = "your_collection_name";
    x.Limit = 1;
});

services.AddMem0DotNetStore(x =>
{
    x.Host = "your_host_address";
});

使用示例
项目包含几个单元测试用例来展示如何使用 Mem0Service 类执行不同的任务。以下是四个主要功能的例子：
保存记忆
await mem0.SaveMemory("unique_id", "明天中午你跟我一起去看电影");

搜索记忆
var result = await mem0.SearchMemory("unique_id");
// 或者带有查询文本
var result = await mem0.SearchMemory("unique_id", "明天中午我们要做什么来着？");

删除记忆
var result = await mem0.DeleteMemory("unique_id");


