namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class RemoveTests : UnitTestBase
    {
        [TestMethod]
        public void Remove_Strings()
        {
            string[] items = ["horse", "dog", "caterpillar", "airplane", "chair"];

            var results = items.Remove(GetModel(), "things that are alive");
            Assert.IsTrue(results.Contains("chair"));
            Assert.IsTrue(results.Contains("airplane"));
            Assert.IsFalse(results.Contains("horse"));
            Assert.IsFalse(results.Contains("dog"));
            Assert.IsFalse(results.Contains("caterpillar"));
        }

        [TestMethod]
        public void Remove_Objects()
        {
            var rnd = new Random();
            string[] items = ["horse", "dog", "caterpillar", "airplane", "chair"];
            var list = items.Select(item => new { Name = item, Age = rnd.Next(1, 100) }).ToList();

            var results = list.Remove(GetModel(), "things that are alive");

            Assert.IsTrue(results.Any(item => item.Name == "chair"));
            Assert.IsTrue(results.Any(item => item.Name == "airplane"));
            Assert.IsFalse(results.Any(item => item.Name == "dog"));
            Assert.IsFalse(results.Any(item => item.Name == "caterpillar"));
            Assert.IsFalse(results.Any(item => item.Name == "horse"));
        }
    }
}