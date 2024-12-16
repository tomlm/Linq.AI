using Iciclecreek.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Linq.AI.OpenAI
{

    /// <summary>
    /// ITransformer for OpenAI
    /// </summary>
    public class OpenAITransformer : ITransformer
    {
        private ChatClient _chatClient;

        internal class Transformation<T>
        {
            [Description("Explain your reasoning")]
            public string? Explanation { get; set; }

            [Description("The result of the goal")]
            public T? Result { get; set; }
        }

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        /// <summary> Initializes a new instance of <see cref="OpenAITransformer">. </summary>
        /// <param name="model"> The name of the model to use in requests sent to the service. To learn more about the available models, see <see href="https://platform.openai.com/docs/models"/>. </param>
        /// <param name="credential"> The API key to authenticate with the service. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="model"/> or <paramref name="credential"/> is null. </exception>
        /// <exception cref="ArgumentException"> <paramref name="model"/> is an empty string, and was expected to be non-empty. </exception>
        public OpenAITransformer(string model, ApiKeyCredential credential) : this(model, credential, new OpenAIClientOptions())
        {
        }

        /// <summary> Initializes a new instance of <see cref="OpenAITransformer">. </summary>
        /// <param name="model"> The name of the model to use in requests sent to the service. To learn more about the available models, see <see href="https://platform.openai.com/docs/models"/>. </param>
        /// <param name="credential"> The API key to authenticate with the service. </param>
        /// <param name="options"> The options to configure the client. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="model"/> or <paramref name="credential"/> is null. </exception>
        /// <exception cref="ArgumentException"> <paramref name="model"/> is an empty string, and was expected to be non-empty. </exception>
        public OpenAITransformer(string model, ApiKeyCredential credential, OpenAIClientOptions options) : this(new ChatClient(model, credential, options))
        {
        }

        public OpenAITransformer(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        /// <summary>
        /// Temperature to use.
        /// </summary>
        public float? Temperature { get; set; } = 0.0f;

        /// <summary>
        /// Tool registrations
        /// </summary>
        public IList<ChatTool> ChatTools { get; } = new List<ChatTool>();

        // map of tool name to the delegate
        private Dictionary<string, ToolDefinition> _tools = new Dictionary<string, ToolDefinition>();

        /// <summary>
        /// Add all static methods on a class as tools.
        /// </summary>
        /// <typeparam name="ToolClassT"></typeparam>
        /// <returns></returns>
        public OpenAITransformer AddTools<ToolClassT>()
            where ToolClassT : class
        {
            foreach (var method in typeof(ToolClassT).GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToList();
                paramTypes.Add(method.ReturnType);

                Type delegateType = Expression.GetDelegateType(paramTypes.ToArray());
                string description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ??
                        method.GetCustomAttribute<InstructionAttribute>()?.Instruction
                        ?? method.Name;
                var priorityGroup = method.GetCustomAttribute<PriorityGroupAttribute>()?.Group ?? 0;
                AddTool(method.Name, description, Delegate.CreateDelegate(delegateType, method, true)!, priorityGroup);
            }
            return this;
        }

        /// <summary>
        /// Add Static Method as a tool.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public OpenAITransformer AddTool(string name, string description, Delegate del, int priorityGroup = 0)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(description);
            ArgumentNullException.ThrowIfNull(del);
            var methodInfo = del.Method;
            string toolName = (methodInfo.DeclaringType!.Name.StartsWith("<")) ? name : $"{methodInfo.DeclaringType.Name}_{name}";

            JObject parmsSchema = new JObject();
            parmsSchema["type"] = "object";
            parmsSchema["additionalProperties"] = false;
            parmsSchema["required"] = new JArray();
            parmsSchema["properties"] = new JObject();

            foreach (var parameter in methodInfo.GetParameters().Where(parm => parm.ParameterType != typeof(CancellationToken) && parm.ParameterType != typeof(CompletionContext)))
            {
                ArgumentNullException.ThrowIfNull(parameter?.Name);

                var parmSchema = StructuredSchemaGenerator.FromType(parameter.ParameterType);
                bool propRequired = parameter.GetCustomAttribute<RequiredAttribute>() != null;

                var instr = parameter.GetCustomAttribute<InstructionAttribute>();
                if (instr != null)
                {
                    parmSchema!["description"] = instr.Instruction;
                }
                else
                {
                    var descr = parameter.GetCustomAttribute<DescriptionAttribute>();
                    if (descr != null)
                        parmSchema!["description"] = descr.Description;
                }
                parmsSchema["properties"]![parameter.Name] = parmSchema;

                ((JArray)parmsSchema["required"]!).Add(parameter.Name);
            }

            var chatTool = ChatTool.CreateFunctionTool(toolName, description, BinaryData.FromString(parmsSchema.ToString()), true);
            if (ChatTools.Any(t => t.FunctionName == chatTool.FunctionName))
                throw new Exception($"{chatTool.FunctionName} is already defined!");

            ChatTools.Add(chatTool);
            _tools[toolName] = new ToolDefinition(chatTool, del, priorityGroup);
            return this;
        }

        /// <summary>
        /// Generate an item of shape T based on "goal"
        /// </summary>
        /// <typeparam name="ResultT">type of items</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="constraint">The constraint to match to remove an item</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public ResultT Generate<ResultT>(string goal, string? instructions = null, CancellationToken cancellationToken = default)
            => TransformItem<ResultT>(String.Empty, goal, instructions, cancellationToken);

        /// <summary>
        /// Generate an item of shape T based on "goal"
        /// </summary>
        /// <typeparam name="T">type of items</typeparam>
        /// <param name="source">collection of items</param>
        /// <param name="constraint">The constraint to match to remove an item</param>
        /// <param name="instructions">(OPTIONAL) additional instructions</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>collection of items which didn't match the goal</returns>
        public Task<ResultT> GenerateAsync<ResultT>(string goal, string? instructions = null, CancellationToken cancellationToken = default)
            => TransformItemAsync<ResultT>(String.Empty, goal, instructions, cancellationToken);

        /// <summary>
        /// Transform text using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public ResultT TransformItem<ResultT>(object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => TransformItemAsync<ResultT>(item, goal, instructions, cancellationToken).Result;

        /// <summary>
        /// Transform item using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public async Task<ResultT> TransformItemAsync<ResultT>(object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Transformation<ResultT>>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat("Transform", jsonSchema: BinaryData.FromString(schema), jsonSchemaIsStrict: true);
            var context = new CompletionContext()
            {
                Item = item,
                Options = new ChatCompletionOptions()
                {
                    ResponseFormat = responseFormat,
                    Temperature = this.Temperature,
                }
            };

            if (ChatTools.Any())
            {
                context.Options.AllowParallelToolCalls = true;
                foreach (var tool in ChatTools)
                    context.Options.Tools.Add(tool);
            }

            context.Messages.Add(GetTransformerSystemPrompt(goal ?? "Transform", instructions));
            context.Messages.Add(GetTransformerItemMessage(item));

            Debug.WriteLine("===============================================");
#if DEBUG
            lock (this)
            {
                foreach (var message in context.Messages)
                {
                    foreach (var part in message.Content)
                        Debug.WriteLine(JsonConvert.SerializeObject(part, JsonSettings));
                }
            }
#endif
            int retries = 2;
            while (retries-- > 0)
            {
                context.Result = await _chatClient.CompleteChatAsync(context.Messages, context.Options, cancellationToken: cancellationToken);

                switch (context.Completion.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        // Add the assistant message to the conversation history.
                        // messages.Add(new AssistantChatMessage(completion));
                        return context.Completion.Content.Select(content =>
                        {
#if DEBUG
                            lock (this)
                            {
                                Debug.WriteLine(content.Text);
                            }
#endif
                            var transformation = JsonConvert.DeserializeObject<Transformation<ResultT>>(content.Text, JsonSettings)!;
                            return transformation.Result!;
                        }).Single()!;

                    case ChatFinishReason.ToolCalls:
                        {
                            // new options with no tools
                            // we only allow tools to be called once to prevent infinite tool invocation
                            // situations.
                            context.Options = new ChatCompletionOptions()
                            {
                                ResponseFormat = responseFormat,
                                Temperature = this.Temperature
                            };

                            // First, add the assistant message with tool calls to the conversation history.
                            context.Messages.Add(new AssistantChatMessage(context.Completion));

                            // group by tool priority
                            foreach (var toolGroup in context.Completion.ToolCalls.Select(call => new ToolInvocation(_tools[call.FunctionName], call))
                                                                                  .GroupBy(inv => inv.Tool.Priority)
                                                                                  .OrderBy(grp => grp.Key))
                            {
                                // process each item in a group in parallel.
                                toolGroup.ForEachParallelAsync(async (toolInvocation, index, ct) =>
                                {
                                    try
                                    {

                                        var result = await toolInvocation.InvokeAsync(context, ct);
                                        lock (context)
                                        {
                                            context.ToolResults[toolInvocation.ToolCall.Id] = result;

                                            if (result != null && result is string s)
                                                context.Messages.Add(new ToolChatMessage(toolInvocation.ToolCall.Id, s));
                                            else
                                                context.Messages.Add(new ToolChatMessage(toolInvocation.ToolCall.Id, JToken.FromObject(result ?? String.Empty).ToString()));
                                        }
                                    }
                                    catch (Exception err)
                                    {
                                        lock (context)
                                        {
                                            context.Messages.Add(new ToolChatMessage(toolInvocation.ToolCall.Id, $"Error occured: {err.Message}"));
                                        }
                                    }
                                });
                            }
                        }
                        break;

                    case ChatFinishReason.Length:
                        throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                    case ChatFinishReason.ContentFilter:
                        throw new NotImplementedException("Omitted content due to a content filter flag.");

                    case ChatFinishReason.FunctionCall:
                        throw new NotImplementedException("Deprecated in favor of tool calls.");

                    default:
                        throw new NotImplementedException(context.Completion.FinishReason.ToString());
                }
            }

            throw new Exception("Too many function calls detected!");
        }

        /// <summary>
        /// Transform items using pool of background tasks and AI model
        /// </summary>
        /// <typeparam name="ResultT">result type</typeparam>
        /// <param name="source">source collection</param>
        /// <param name="model">ai model to use</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="maxParallel">(OPTIONAL) max parallel operations</param>
        /// <param name="cancellationToken">(OPTIONAL) cancellation token</param>
        /// <returns>transformed results</returns>
        public IList<ResultT> TransformItems<ResultT>(IEnumerable<object> source, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Transformation<ResultT>>().ToString();
            var count = source.Count();

            return source.SelectParallelAsync((item, index, ct) =>
                    TransformItemAsync<ResultT>(item, goal, Utils.GetItemIndexClause((int)index, (int)count, instructions), cancellationToken),
                    maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken);
        }

        internal static SystemChatMessage GetTransformerSystemPrompt(string goal, string? instructions = null)
        {
            return new SystemChatMessage($$"""
                    You are an expert at transforming.

                    <GOAL>
                    {{goal}}

                    <INSTRUCTIONS>
                    Transform <ITEM> using the directions in the provided <GOAL>.
                    {{instructions}}
                    """);
        }

        internal static UserChatMessage GetTransformerItemMessage(object item)
        {
            if (item is string)
            {
                return new UserChatMessage($$"""
                <ITEM>
                {{item}}
                """);
            }
            else if (item is ChatMessageContentPart contentPart)
            {
                if (contentPart.Kind == ChatMessageContentPartKind.Image)
                    return new UserChatMessage(ChatMessageContentPart.CreateTextPart("<ITEM>"), contentPart);
                else
                    return new UserChatMessage($$"""
                    <ITEM>
                    {{contentPart.Text}}
                    """);
            }
            else if (item is ChatMessageContentPart[] contentParts)
            {
                return new UserChatMessage(contentParts);
            }
            else if (item is Uri uri)
            {
                return new UserChatMessage(ChatMessageContentPart.CreateTextPart("<ITEM>"), ChatMessageContentPart.CreateImagePart(uri));
            }
            else if (item is Uri[] uris)
            {
                List<ChatMessageContentPart> parts = new List<ChatMessageContentPart>()
                {
                    ChatMessageContentPart.CreateTextPart("<ITEM>")
                };

                foreach (var u in uris)
                {
                    parts.Add(ChatMessageContentPart.CreateImagePart(u));
                }
                return new UserChatMessage(parts);
            }
            else
            {
                return new UserChatMessage($$"""
                    <ITEM>
                    {{JToken.FromObject(item).ToString()}}
                    """);
            }
        }

    }
}
