using Iciclecreek.Async;

namespace Linq.AI.OpenAI.Tests
{

    [TestClass]
    public partial class SummarizeTests : UnitTestBase
    {
        public List<string> GetDocs()
        {
            string[] urls = [
    "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/Architecture.md",
                "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/CardApp.md",
                "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/CardView.md"
            ];
            HttpClient httpClient = new HttpClient();
            return urls.SelectParallelAsync(async (url, i) => await httpClient.GetStringAsync(url)).ToList();
        }

        [TestMethod]
        public async Task String_Summarize()
        {
            var summarization = await Text.SummarizeAsync(Model, "2 words");
            Assert.IsTrue(summarization.Contains("Hope"));
            Assert.IsTrue(summarization.Contains("Change"));
        }

        [TestMethod]
        public void Summarize_Strings()
        {
            var docs = GetDocs();
            foreach (var result in docs.Summarize(Model))
            {
                Assert.IsNotNull(result);
            }

            foreach (var result in docs.Summarize(Model, "Create a 3 bullet summary"))
            {
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void Summarize_Objects()
        {
            var docs = GetDocs().Select(markdown => new TestObject() { Item = markdown }).ToList();
            foreach (var result in docs.Summarize(Model))
            {
                Assert.IsNotNull(result);
            }

        }
    }
}