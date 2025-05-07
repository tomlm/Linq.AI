using Newtonsoft.Json;
using System.Diagnostics;

namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class ToolsTests : UnitTestBase
    {

        public override ITransformer GetModel(string modelName = "gpt-4o")
        {
            var model = base.GetModel(modelName) as OpenAITransformer;

            model
                .AddTools<MyFunctions>()
                .AddTool("LookupContact", "lookup a contact record for a person",
                    async (string name, CancellationToken ct) =>
                    {
                        await Task.Delay(500);
                        return new TestContact()
                        {
                            Name = "John Smith III",
                            HomeTown = "Atlanta"
                        };
                    }
                );
            for (char ch = 'A'; ch < 'Z'; ch++)
            {
                var result = $"{ch}";
                model.AddTool($"FUNC{ch}", $"Function for FUNC{ch}()", async () =>
                {
                    await Task.Delay(1000);
                    return result;
                }, (ch % 3) - 1);
            }
            return model;
        }

        [TestMethod]
        public async Task Tool_SingleFunctionTest()
        {
            var result = await GetModel().GenerateAsync<double>("what is magic result for 3 and 4?");
            Assert.AreEqual(81f, result);
        }

        [TestMethod]
        public async Task Tool_SingleFunctionStringTest()
        {
            var result = await GetModel().GenerateAsync<string>("what is the weather in Ames, Iowa?");
            Assert.IsTrue(await GetModel().CompareAsync("100 farenheit", result));
        }

        [TestMethod]
        public async Task Tool_SingleFunctionStringTest2()
        {
            var result = await GetModel().GenerateAsync<string>("what is the weather in Ames, Iowa in celcius?");
            Assert.IsTrue(await GetModel().CompareAsync("37 degress celsius", result));
        }

        [TestMethod]
        public async Task Tool_MultipleFunctionStringTest()
        {
            var result = await GetModel().GenerateAsync<string>("what is the weather in Ames, Iowa? What is the magic result for 3 and 4?");
            Assert.IsTrue(await GetModel().CompareAsync("The weather in Ames, Iowa is 100°F. The magic result for 3 and 4 is 81.", result));
        }

        [TestMethod]
        public async Task Tool_DelegateTest()
        {
            var result = await GetModel().GenerateAsync<string>("Look up John's contact");
            Assert.IsTrue(result.Contains("John"));
            Assert.IsTrue(result.Contains("Smith"));
        }


        [TestMethod]
        public async Task Tool_PriorityGroupText()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = await GetModel().GenerateAsync<string[]>("return the results of FUNCA() and FUNCF() as results");
            sw.Stop();
            Assert.IsTrue(result.Contains("A"));
            Assert.IsFalse(result.Contains("B"));
            Assert.IsFalse(result.Contains("C"));
            Assert.IsFalse(result.Contains("D"));
            Assert.IsFalse(result.Contains("E"));
            Assert.IsTrue(result.Contains("F"));
        }

        [TestMethod]
        public async Task Tool_ItemContext()
        {
            var item = new TestItem { Name = "A", Counter = 0 };
            var result = await GetModel().QueryAbout<TestItem>(item, "increment the item counter");
            Assert.AreEqual(1, result.Counter);
            Assert.AreEqual(JsonConvert.SerializeObject(item), JsonConvert.SerializeObject(result));
        }

    }


}