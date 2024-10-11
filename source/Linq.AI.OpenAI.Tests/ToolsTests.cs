using System.ComponentModel;

namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class ToolsTests : UnitTestBase
    {

        [TestMethod]
        public async Task Tool_SingleFunctionTest()
        {
            var result = await Model.GenerateAsync<double>("what is magic result for 3 and 4?");
            Assert.AreEqual(81f, result);
        }

        [TestMethod]
        public async Task Tool_SingleFunctionStringTest()
        {
            var result = await Model.GenerateAsync<string>("what is the weather in Ames, Iowa?");
            Assert.IsTrue(await Model.CompareAsync("100 farenheit", result));
        }

        [TestMethod]
        public async Task Tool_SingleFunctionStringTest2()
        {
            var result = await Model.GenerateAsync<string>("what is the weather in Ames, Iowa in celcius?");
            Assert.IsTrue(await Model.CompareAsync("37 degress celsius", result));
        }

        [TestMethod]
        public async Task Tool_MultipleFunctionStringTest()
        {
            var result = await Model.GenerateAsync<string>("what is the weather in Ames, Iowa? What is the magic result for 3 and 4?");
            Assert.IsTrue(await Model.CompareAsync("The weather in Ames, Iowa is 100°F. The magic result for 3 and 4 is 81.", result));
        }

        [TestMethod]
        public async Task Tool_DelegateTest()
        {
            var result = await Model.GenerateAsync<string>("Look up John's contact");
            Assert.IsTrue(result.Contains("John Smith III"));
        }
    }
}

 