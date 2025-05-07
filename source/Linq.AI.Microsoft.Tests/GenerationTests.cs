using System.ComponentModel;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Linq.AI.Microsoft.Tests
{


    [TestClass]
    public class GenerationTests : UnitTestBase
    {

        [TestMethod]
        public async Task Generate_Text()
        {
            var results = await GetModel().GenerateAsync("a haiku about camping", "it must have the word camp, campfire or camping in it");
            Assert.IsTrue(results.ToLower().Contains("camp"));
        }

        [TestMethod]
        public async Task Generate_Object()
        {
            var result = await GetModel().GenerateAsync<CityObject>("a city object for U.S. city Ames, Iowa");
            Assert.AreEqual("Ames", result.Name);
        }

        [TestMethod]
        public async Task Generate_Collection_Text()
        {
            var results = await GetModel().GenerateAsync<string[]>("return the top 5 largest cities in the world");
            Assert.IsTrue(results.Any(item => item.Contains("Tokyo")));
            Assert.AreEqual(5, results.Count());
        }

        [TestMethod]
        public async Task Generate_Collection_Text2()
        {
            var results = await GetModel().GenerateAsync<string[]>("a list of 5 funny names for people named bob", "the name must include bob in it.");
            foreach(var result in results)
            {
                Assert.IsTrue(result.ToLower().Contains("bob"));
            }
        }

        [TestMethod]
        public async Task Generate_Collections_Object()
        {
            var generated = await GetModel().GenerateAsync<CityObject[]>("return the top 5 largest cities in the world.");
            Assert.IsTrue(generated.Any(item => item.Name == "Tokyo" && item.Country == "Japan" ));
            Assert.AreEqual(5, generated.Count());
        }

        [TestMethod]
        public async Task Transformer_StringDefault()
        {
            Assert.AreEqual("tset", await GetModel().TransformItem("test", "reverse letters"));
        }

    }

    internal class CityObject
    {
        [System.ComponentModel.Description("Name of the city")]
        public string? Name { get; set; }

        [System.ComponentModel.Description("Country name")]
        public string? Country { get; set; }
    }
}