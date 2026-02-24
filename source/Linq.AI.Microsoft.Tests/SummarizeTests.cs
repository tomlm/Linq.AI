using System.Diagnostics;

namespace Linq.AI.Microsoft.Tests
{

    [TestClass]
    public partial class SummarizeTests : UnitTestBase
    {
        public async IAsyncEnumerable<string> GetDocs()
        {
            string[] urls = [
    "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/Architecture.md",
                "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/CardApp.md",
                "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/CardView.md"
            ];
            HttpClient httpClient = new HttpClient();
            foreach (var url in urls)
                yield return await httpClient.GetStringAsync(url);
        }

        [TestMethod]
        public async Task Summarize_String()
        {
            var summarization = await GetModel().SummarizeAsync(Text, "Create a single sentence describing this content as a title.");
            Assert.IsTrue(summarization.Contains("presidency", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task Summarize_Strings()
        {
            var docs = GetDocs();
            await foreach (var result in docs.SummarizeAsync(GetModel()))
            {
                Debug.WriteLine(result);
                Assert.IsNotNull(result);
            }

            await foreach (var result in docs.SummarizeAsync(GetModel(), "Create a 3 bullet summary"))
            {
                Debug.WriteLine(result);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public async Task Summarize_Objects()
        {
            await foreach (var result in GetDocs()
                            .Select(markdown => new TestObject() { Name = markdown })
                            .SummarizeAsync(GetModel()))
            {
                Debug.WriteLine(result);
                Assert.IsNotNull(result);
            }
        }
    }
}