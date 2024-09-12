using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linq.AI.OpenAI
{
    internal static class Utils
    {
        internal static UserChatMessage GetItemPrompt(object item, int index, int length)
        {
            if (!(item is string))
            {
                item = JToken.FromObject(item).ToString();
            }
            return new UserChatMessage($$"""
            <INDEX>
            Item Index: {{index}} 
            Total Item Count: {{length}}

            <ITEM>
            {{item}}
            """);
        }

        internal static UserChatMessage GetItemPrompt(object item)
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