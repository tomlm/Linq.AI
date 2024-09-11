namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class RemoveTests : UnitTestBase
    {
        [TestMethod]
        public void Remove_Strings()
        {
            string[] items = ["horse", "dog", "caterpillar", "airline", "chair"];

            var results = items.RemoveAI(ChatClient, "things you can travel on").ToList();
            Assert.IsFalse(results.Contains("horse"));
            Assert.IsFalse(results.Contains("airline"));
            Assert.IsTrue(results.Contains("dog"));
            Assert.IsTrue(results.Contains("caterpillar"));
            Assert.IsTrue(results.Contains("chair"));
        }

        [TestMethod]
        public void Remove_Objects()
        {
            string[] items = ["horse", "dog", "caterpillar", "airline", "chair"];

            var results = items.Select(item => new { Name = item }).RemoveAI(ChatClient, "it is something you can ride").ToList();
            Assert.IsFalse(results.Any(item => item.Name == "horse"));
            Assert.IsFalse(results.Any(item => item.Name == "airline"));
            Assert.IsTrue(results.Any(item => item.Name == "dog"));
            Assert.IsTrue(results.Any(item => item.Name == "caterpillar"));
            Assert.IsTrue(results.Any(item => item.Name == "chair"));
        }
    }
}