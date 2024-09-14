﻿using Iciclecreek.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System.ComponentModel;
using System.Diagnostics;

namespace Linq.AI.OpenAI
{
    internal class Transformation<T>
    {
        [Description("Explain your reasoning")]
        public string? Explanation { get; set; }

        [Description("The result of the goal")]
        public T? Result { get; set; }
    }

    public static class TransformExtension
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        /// <summary>
        /// Transform text using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public static ResultT TransformItem<ResultT>(this ChatClient model, object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(item, goal, instructions, cancellationToken).Result;

        /// <summary>
        /// Transform text using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public async static Task<ResultT> TransformItemAsync<ResultT>(this ChatClient model, object item, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Transformation<ResultT>>().ToString();
            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "Transform", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
            ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
            var systemChatMessage = GetSystemPrompt(goal ?? "Transform", instructions);
            var itemMessage = GetItemMessage(item);
            ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options, cancellationToken: cancellationToken);
            return chatCompletion.Content.Select(completion =>
            {
#if DEBUG
                lock (model)
                {
                    Debug.WriteLine("===============================================");
                    Debug.WriteLine(systemChatMessage.Content.Single().Text);
                    Debug.WriteLine(itemMessage.Content.Single().Text);
                    Debug.WriteLine(completion.Text);
                }
#endif
                var transformation = JsonConvert.DeserializeObject<Transformation<ResultT>>(completion.Text, JsonSettings)!;
                return transformation.Result!;
            }).Single()!;
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
        public static IList<ResultT> TransformItems<ResultT>(this ChatClient model, IEnumerable<object> source, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
        {
            var schema = StructuredSchemaGenerator.FromType<Transformation<ResultT>>().ToString();
            var count = source.Count();

            return source.SelectParallelAsync(async (item, index, ct) =>
            {
                var itemResult = item;
                var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(name: "transform", jsonSchema: BinaryData.FromString(schema), strictSchemaEnabled: true);
                ChatCompletionOptions options = new ChatCompletionOptions() { ResponseFormat = responseFormat, };
                var systemChatMessage = GetSystemPrompt(goal ?? "transform the item to the output schema", instructions!, index, count);
                var itemMessage = GetItemMessage(itemResult!);
                ChatCompletion chatCompletion = await model.CompleteChatAsync([systemChatMessage, itemMessage], options, ct);
                return chatCompletion.Content.Select(completion =>
                {
#if DEBUG
                    lock (source)
                    {
                        Debug.WriteLine("===============================================");
                        Debug.WriteLine(systemChatMessage.Content.Single().Text);
                        Debug.WriteLine(itemMessage.Content.Single().Text);
                        Debug.WriteLine(completion.Text);
                    }
#endif
                    var transformation = JsonConvert.DeserializeObject<Transformation<ResultT>>(completion.Text, JsonSettings)!;
                    return transformation.Result;
                }).Single()!;
            }, maxParallel: maxParallel ?? 2 * Environment.ProcessorCount, cancellationToken);
        }

        /// <summary>
        /// Transform text using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public static ResultT TransformItem<ResultT>(this object item, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(item, goal, instructions, cancellationToken).Result;

        /// <summary>
        /// Transform text using OpenAI model
        /// </summary>
        /// <param name="item">item to Transform</param>
        /// <param name="model">ChatClient to use as model</param>
        /// <param name="goal">(OPTIONAL) Goal for what you want to Transform</param>
        /// <param name="instructions">(OPTIONAL) additional instructions for how to transform</param>
        /// <param name="cancellationToken">(OPTIONAL) Cancellation Token</param>
        /// <returns>transformed text</returns>
        public static Task<ResultT> TransformItemAsync<ResultT>(this object item, ChatClient model, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<ResultT>(item, goal, instructions, cancellationToken);

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
        public static IList<ResultT> TransformItems<ResultT>(this IEnumerable<object> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => model.TransformItems<ResultT>(source, goal, instructions, maxParallel, cancellationToken);

        internal static SystemChatMessage GetSystemPrompt(string goal, string? instructions = null, int? index = null, int? count = null)
        {
            if (index != null && count != null)
            {
                instructions = GetItemIndexClause((int)index, (int)count, instructions);
            }
            return new SystemChatMessage($$"""
                    You are an expert at transforming text.

                    <GOAL>
                    {{goal}}

                    <INSTRUCTIONS>
                    Given <ITEM> text transform the text using the directions in the provided <GOAL>.
                    {{instructions}}
                    """);
        }

        internal static string GetItemIndexClause(int index, int count, string? instructions)
        {
            return $"""
                    First Index: 0
                    Last Index: {count-1}
                    This Item Index: {index} 
                    {instructions ?? string.Empty}
                    """;
        }


        internal static UserChatMessage GetItemMessage(object item)
        {
            if (!(item is string))
            {
                item = JToken.FromObject(item).ToString();
            }
            return new UserChatMessage($$"""
            <ITEM>
            {{item}}
            """);
        }

    }
}
