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
            string[] items = ["horse", "dog", "caterpillar", "airline", "chair"];

            var results = items.Where(Model, "keep things you can travel on");
            Assert.IsTrue(results.Contains("horse"));
            Assert.IsTrue(results.Contains("airline"));
            Assert.IsFalse(results.Contains("dog"));
            Assert.IsFalse(results.Contains("caterpillar"));
            Assert.IsFalse(results.Contains("chair"));
        }

        [TestMethod]
        public void Where_Objects()
        {
            string[] items = ["horse", "dog", "caterpillar", "airline", "chair"];

            var results = items.Select(item => new { Name = item }).Where(Model, "it is something you can ride");
            Assert.IsTrue(results.Any(item => item.Name == "horse"));
            Assert.IsTrue(results.Any(item => item.Name == "airline"));
            Assert.IsFalse(results.Any(item => item.Name == "dog"));
            Assert.IsFalse(results.Any(item => item.Name == "caterpillar"));
            Assert.IsFalse(results.Any(item => item.Name == "chair"));
        }
    }
}