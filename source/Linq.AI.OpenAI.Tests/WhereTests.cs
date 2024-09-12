namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class WhereTests : UnitTestBase
    {

        [TestMethod]
        public async Task Matches()
        {
            Assert.IsTrue(await "a duck".MatchesAsync(Model, "a bird"));
            Assert.IsFalse(await "a truck".MatchesAsync(Model, "a bird"));
        }

        [TestMethod]
        public void Where_Strings()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Where(Model, "keep things you can travel on");
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsTrue(results.Contains("airplane"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsFalse(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("sandwich"));
        }

        [TestMethod]
        public void Where_Objects()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Select(item => new { Name = item }).Where(Model, "it is something you can ride");
            Assert.IsTrue(results.Any(item => item.Name == "horse"));
            Assert.IsTrue(results.Any(item => item.Name == "airplane"));
            Assert.IsFalse(results.Any(item => item.Name == "tack"));
            Assert.IsFalse(results.Any(item => item.Name == "caterpillar"));
            Assert.IsFalse(results.Any(item => item.Name == "sandwich"));
        }

        [TestMethod]
        public void Where_Index_Semantic()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Where(Model, "The item is the first, third or last");
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsTrue(results.Contains("sandwich"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsFalse(results.Contains("airplane"));
        }

        [TestMethod]
        public void Where_Index()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Where(Model, "The item index is even");
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsTrue(results.Contains("sandwich"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsFalse(results.Contains("airplane"));
        }
    }
}