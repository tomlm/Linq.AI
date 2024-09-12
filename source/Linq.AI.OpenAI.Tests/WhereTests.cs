namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class WhereTests : UnitTestBase
    {

        [TestMethod]
        public async Task Matches()
        {
            Assert.IsTrue(await "a duck".MatchesAsync(Model, "item is a bird"));
            Assert.IsFalse(await "a truck".MatchesAsync(Model, "item is a bird"));
        }

        [TestMethod]
        public void Where_Strings()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Where(Model, "item is a thing you can travel on");
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

            var results = items.Select(item => new { Name = item }).Where(Model, "item is something you can ride");
            Assert.IsTrue(results.Any(item => item.Name == "horse"));
            Assert.IsFalse(results.Any(item => item.Name == "tack"));
            Assert.IsTrue(results.Any(item => item.Name == "airplane"));
            Assert.IsFalse(results.Any(item => item.Name == "caterpillar"));
            Assert.IsFalse(results.Any(item => item.Name == "sandwich"));
        }

        [TestMethod]
        public void Where_Index_Semantic()
        {
            string[] items = ["horse", "tack", "caterpillar", "airplane", "sandwich"];

            var results = items.Where(Model, "The item is the first or last");
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

            var results = items.Where(Model, "The item index is even number");
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsFalse(results.Contains("tack"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("airplane"));
            Assert.IsTrue(results.Contains("sandwich"));
        }
    }
}