using System.ComponentModel;

namespace Linq.AI.OpenAI.Tests
{


    [TestClass]
    public class GenerationTests : UnitTestBase
    {

        [TestMethod]
        public void Generate_Text()
        {
            var results = Model.Generate("a haiku about camping", "it must have the word camp, campfire or camping in it");
            Assert.IsTrue(results.ToLower().Contains("camp"));
        }

        [TestMethod]
        public void Generate_Object()
        {
            var result = Model.Generate<CityObject>("a city object for Ames, Iowa");
            Assert.AreEqual("Ames", result.Name);
        }

        [TestMethod]
        public async Task Generate_Collection_Text()
        {
            var results = await Model.GenerateAsync<string[]>("return the top 5 largest cities in the world");
            Assert.IsTrue(results.Any(item => item.Contains("Mexico City")));
            Assert.AreEqual(5, results.Count());
        }

        [TestMethod]
        public async Task Generate_Collection_Text2()
        {
            var results = await Model.GenerateAsync<string[]>("a list of funny names for people named bob");
            foreach(var result in results)
            {
                Assert.IsTrue(result.ToLower().Contains("bob"));
            }
        }



        [TestMethod]
        public void Generate_Collections_Object()
        {
            var generated = Model.Generate<CityObject[]>("return the top 5 largest cities in the world.");
            Assert.IsTrue(generated.Any(item => item.Name == "Mexico City" && item.Country == "Mexico" ));
            Assert.AreEqual(5, generated.Count());
        }

        [TestMethod]
        public async Task Transformer_StringDefault()
        {
            Assert.AreEqual("tset", Model.TransformItem("test", "reverse letters"));
            Assert.AreEqual("tset", await Model.TransformItemAsync("test", "reverse letters"));
        }

    }

    internal class CityObject
    {
        [System.ComponentModel.Description("Name of the city")]
        public string? Name { get; set; }

        [System.ComponentModel.Description("Name of the Country")]
        public string? Country { get; set; }
    }
}