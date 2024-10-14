
namespace Linq.AI.OpenAI.Tests
{

    [TestClass]
    public class CompareTests : UnitTestBase
    {
        [TestMethod]
        public void Compare_Strings_Semantically()
        {
            Assert.IsTrue(GetModel().Compare("fourteen", "14"));
            Assert.IsTrue(GetModel().Compare("fourteen years old", "10 + 4 years"));
            Assert.IsTrue(GetModel().Compare("Me llamo Tom", "Mi nombre es Tom"));
            Assert.IsTrue(GetModel().Compare("My name is Tom", "Mi nombre es Tom", instructions: "allow different langauges to be semantically equal"));
            Assert.IsFalse(GetModel().Compare("Me llamo Tom", "Mi padre es Tom"));
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