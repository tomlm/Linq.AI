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
            return urls.SelectParallelAsync(async (url, i, ct) => await httpClient.GetStringAsync(url, ct));
        }

        [TestMethod]
        public async Task Summarize_String()
        {
            var summarization = Model.Summarize(Text, "2 words");
            foreach(var summary in summarization)
            {
                Debug.WriteLine(summarization);
            }

            Assert.IsTrue(summarization.Contains("Hope"));
            Assert.IsTrue(summarization.Contains("Change"));
            
            summarization = await Model.SummarizeAsync(Text, "2 words");
            foreach (var summary in summarization)
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
            var docs = GetDocs().Select(markdown => new TestObject() { Name = markdown }).ToList();
            foreach (var result in docs.Summarize(Model))
            {
                Debug.WriteLine(result);
                Assert.IsNotNull(result);
            }

        }
    }
}