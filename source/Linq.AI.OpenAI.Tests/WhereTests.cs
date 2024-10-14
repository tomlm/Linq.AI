namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class WhereTests : UnitTestBase
    {

        [TestMethod]
        public async Task Matches()
        {
            Assert.IsTrue(GetModel().Matches("a duck", "item is a bird"));
            Assert.IsFalse(GetModel().Matches("a truck", "item is a bird"));

            Assert.IsTrue(await GetModel().MatchesAsync("a duck", "item is a bird"));
            Assert.IsFalse(await GetModel().MatchesAsync("a truck", "item is a bird"));
        }

        [TestMethod]
        public void Where_Strings()
        {
            string[] items = ["horse", "thumb tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Where(GetModel(), "it is alive");
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsFalse(results.Contains("airplane"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("sandwich"));
        }

        [TestMethod]
        public void Where_Objects()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Select(item => new { Name = item }).Where(GetModel(), "it is alive");
            Assert.IsTrue(results.Any(item => item.Name == "horse"));
            Assert.IsFalse(results.Any(item => item.Name == "tack"));
            Assert.IsFalse(results.Any(item => item.Name == "airplane"));
            Assert.IsTrue(results.Any(item => item.Name == "caterpillar"));
            Assert.IsFalse(results.Any(item => item.Name == "sandwich"));
        }

        [TestMethod]
        public void Where_Index_Semantic()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Where(GetModel(), "The item is the first or last");
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsFalse(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("airplane"));
            Assert.IsTrue(results.Contains("sandwich"));
        }

        [TestMethod]
        public void Where_Index()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Where(GetModel(), "The item index is even number");
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("airplane"));
            Assert.IsTrue(results.Contains("sandwich"));
        }
    }
}