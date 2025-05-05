namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class WhereTests : UnitTestBase
    {

        [TestMethod]
        public async Task Matches()
        {
            Assert.IsTrue(await GetModel().MatchesAsync("a duck", "item is a bird"));
            Assert.IsFalse(await GetModel().MatchesAsync("a truck", "item is a bird"));
        }

        [TestMethod]
        public async Task Where_Strings()
        {
            string[] items = ["horse", "thumb tack", "caterpillar", "airplane", "sandwich"];

            var results = await items.WhereAsync(GetModel(), "it is alive").ToListAsync();
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsFalse(results.Contains("airplane"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("sandwich"));
        }

        [TestMethod]
        public async Task Where_Objects()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = await items.Select(item => new { Name = item })
                    .WhereAsync(GetModel(), "it is alive")
                    .ToListAsync();
            Assert.IsTrue(results.Any(item => item.Name == "horse"));
            Assert.IsFalse(results.Any(item => item.Name == "tack"));
            Assert.IsFalse(results.Any(item => item.Name == "airplane"));
            Assert.IsTrue(results.Any(item => item.Name == "caterpillar"));
            Assert.IsFalse(results.Any(item => item.Name == "sandwich"));
        }

        [TestMethod]
        public async Task Where_Index_Semantic()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = await items
                                .WhereAsync(GetModel(), "The item is the second item")
                                .ToListAsync();
            Assert.IsFalse(results.Contains("horse"));
            Assert.IsTrue(results.Contains("tack"));
            Assert.IsFalse(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("airplane"));
            Assert.IsFalse(results.Contains("sandwich"));
        }

        [TestMethod]
        public async Task Where_Index()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = await items
                                .WhereAsync(GetModel(), "The item index is even number")
                                .ToListAsync();
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("airplane"));
            Assert.IsTrue(results.Contains("sandwich"));
        }
    }
}