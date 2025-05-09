using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Linq.AI.OpenAI
{
    [DebuggerDisplay("{Tool.ChatTool.FunctionName}()")]
    internal class ToolDefinition
    {
        internal ToolDefinition(ChatTool chatTool, Delegate del, int priority)
        {
            ChatTool = chatTool;
            Priority = priority;
            Delegate = del;
        }

        public int Priority { get; set; }

        public ChatTool ChatTool { get; set; }

        public Delegate Delegate { get; set; }
    }

    [DebuggerDisplay("{Tool.ChatTool.FunctionName}({ToolCall.FunctionArguments.ToString()})")]
    internal class ToolInvocation
    {
        internal ToolInvocation(ToolDefinition tool, ChatToolCall call)
        {
            Tool = tool;
            ToolCall = call;
        }

        public ChatToolCall ToolCall { get; set; }

        public ToolDefinition Tool { get; set; }

        public async Task<object> InvokeAsync(CompletionContext context, CancellationToken ct)
        {
            var input = !String.IsNullOrEmpty(ToolCall.FunctionArguments.ToString()) ? JObject.Parse(ToolCall.FunctionArguments.ToString()) : new JObject();

            List<object> args = new List<object>();
            foreach (var parameter in Tool.Delegate.Method.GetParameters())
            {
                if (parameter.ParameterType == typeof(CancellationToken))
                {
                    args.Add(ct);
                }
                else if (parameter.ParameterType == typeof(CompletionContext))
                {
                    args.Add(context);
                }
                else
                {
                    var val = input[parameter.Name!]?.ToObject(parameter.ParameterType) ?? parameter.DefaultValue;
                    args.Add(val!);
                }
            }

            // call function()
            object? result = null;
            if (Tool.Delegate.Method.ReturnType.Name == "Task")
            {
                await (Task)Tool.Delegate.DynamicInvoke(args.ToArray<object?>())!;
            }
            else if (Tool.Delegate.Method.ReturnType.Name == "Task`1")
            {
                var task = (Task)Tool.Delegate.DynamicInvoke(args.ToArray<object?>())!;
                if (task != null)
                {
                    await (Task)task;
                    result = task!.GetType().GetProperty("Result", BindingFlags.FlattenHierarchy |
                                                                   BindingFlags.Public |
                                                                   BindingFlags.Instance)!.GetValue(task);
                }
            }
            else
            {
                result = Tool.Delegate.DynamicInvoke(args.ToArray<object?>());
            }
            return result;
        }
    }
}
