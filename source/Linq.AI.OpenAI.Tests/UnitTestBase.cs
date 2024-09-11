using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Linq.AI.OpenAI.Tests
{
    public class UnitTestBase
    {
        private static Lazy<ChatClient> client = new Lazy<ChatClient>(() =>
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<ClassifyTests>()
                .Build();
            return new ChatClient(model: "gpt-4o-2024-08-06", config["OpenAIKey"]);
        });

        public static ChatClient ChatClient
            => client.Value;
    }
}
