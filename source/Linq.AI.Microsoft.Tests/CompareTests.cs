
namespace Linq.AI.Microsoft.Tests
{

    [TestClass]
    public class CompareTests : UnitTestBase
    {
        [TestMethod]
        public async Task Compare_Strings_Semantically()
        {
            Assert.IsTrue(await GetModel().CompareAsync("fourteen", "14"));
            Assert.IsTrue(await GetModel().CompareAsync("fourteen years old", "10 + 4 years"));
            Assert.IsTrue(await GetModel().CompareAsync("Me llamo Tom", "Mi nombre es Tom"));
            Assert.IsTrue(await GetModel().CompareAsync("My name is Tom", "Mi nombre es Tom", instructions: "allow different langauges to be semantically equal"));
            Assert.IsFalse(await GetModel().CompareAsync("Me llamo Tom", "Mi padre es Tom"));
        }

        [TestMethod]
        public async Task Compare_Objects_Semantically()
        {
            Assert.IsTrue(await GetModel().CompareAsync(
                new 
                { 
                    Introduction = "My name is Tom", 
                    Background="I live in Kirkland, Washington"
                },
                new
                {
                    Introduction = "I'm Tom",
                    Background = "I'm from Kirkland, Washington"
                }));
        }

    }
}