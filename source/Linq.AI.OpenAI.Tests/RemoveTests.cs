namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class RemoveTests : UnitTestBase
    {
        [TestMethod]
        public void Remove_Strings()
        {
            string[] items = ["horse", "dog", "caterpillar", "airplane", "chair"];

            var results = items.Remove(Model, "things you can travel on").ToList();
            Assert.IsFalse(results.Contains("horse"));
            Assert.IsFalse(results.Contains("airplane"));
            Assert.IsTrue(results.Contains("dog"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsTrue(results.Contains("chair"));
        }

        [TestMethod]
        public void Remove_Objects()
        {
            string[] items = ["horse", "dog", "caterpillar", "airplane", "chair"];
            var list = items.Select(item => new { Name = item }).ToList();

            var results = list.Remove(Model, "it is something you can ride").ToList();

            Assert.IsFalse(results.Any(item => item.Name == "horse"));
            Assert.IsFalse(results.Any(item => item.Name == "airplane"));
            Assert.IsTrue(results.Any(item => item.Name == "dog"));
            Assert.IsTrue(results.Any(item => item.Name == "caterpillar"));
            Assert.IsTrue(results.Any(item => item.Name == "chair"));
        }
    }
}