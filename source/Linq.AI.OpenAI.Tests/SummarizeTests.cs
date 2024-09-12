using Iciclecreek.Async;
using System.Diagnostics;

namespace Linq.AI.OpenAI.Tests
{

    [TestClass]
    public partial class SummarizeTests : UnitTestBase
    {
        public IList<string> GetDocs()
        {
            string[] urls = [
    "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/Architecture.md",
                "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/CardApp.md",
                "https://raw.githubusercontent.com/tomlm/Crazor/main/docs/CardView.md"
            ];
            HttpClient httpClient = new HttpClient();
            return urls.SelectParallelAsync(async (url, i) => await httpClient.GetStringAsync(url));
        }

        [TestMethod]
        public async Task String_Summarize()
        {
            var summarization = await Text.SummarizeAsync(Model, "2 words");
            foreach(var summary in summarization)
            {
                Debug.WriteLine(summarization);
            }

            Assert.IsTrue(summarization.Contains("Hope"));
            Assert.IsTrue(summarization.Contains("Change"));
        }

        [TestMethod]
        public void Summarize_Strings()
        {
            var docs = GetDocs();
            foreach (var result in docs.Summarize(Model))
            {
                Debug.WriteLine(result);
                Assert.IsNotNull(result);
            }

            foreach (var result in docs.Summarize(Model, "Create a 3 bullet summary"))
            {
                Debug.WriteLine(result);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void Summarize_Objects()
        {
            var docs = GetDocs().Select(markdown => new TestObject() { Item = markdown }).ToList();
            foreach (var result in docs.Summarize(Model))
            {
                Debug.WriteLine(result);
                Assert.IsNotNull(result);
            }

        }
    }
}