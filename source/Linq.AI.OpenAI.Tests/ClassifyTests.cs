
namespace Linq.AI.OpenAI.Tests
{

    public enum TestCategories { Car, Bike, Train, Plane };

    [TestClass]
    public class ClassifyTests : UnitTestBase
    {
        public static string[] Categories = ["Car", "Bike", "Train", "Plane"];

        [TestMethod]
        public async Task Classify_Text_Enum()
        {
            var result = await "Ford".ClassifyAsync<TestCategories>(Model);
            Assert.AreEqual(TestCategories.Car, result);
        }

        [TestMethod]
        public async Task Classifiy_Text_Strings()
        {
            var result = await "Ford".ClassifyAsync(Model, Categories);
            Assert.AreEqual("Car", result);
        }

        [TestMethod]
        public void Classify_Collection_Strings()
        {
            string[] items = ["Super hornet", "Orient Express", "Ford", "puch"];

            foreach (var result in items.Classify(Model, Categories))
            {
                switch (result.Item)
                {
                    case "Super hornet":
                        Assert.AreEqual("Plane", result.Category);
                        break;
                    case "Orient Express":
                        Assert.AreEqual("Train", result.Category);
                        break;
                    case "Ford":
                        Assert.AreEqual("Car", result.Category);
                        break;
                    case "puch":
                        Assert.AreEqual("Bike", result.Category);
                        break;
                }
            }
        }

        [TestMethod]
        public void Classify_Collection_Objects()
        {
            string[] items = ["Super hornet", "Orient Express", "nash", "puch"];

            foreach (var result in items.Select(name => new TestObject() { Name = name }).Classify(Model, Categories))
            {
                switch (result.Item.Name)
                {
                    case "Super hornet":
                        Assert.AreEqual("Plane", result.Category);
                        break;
                    case "Orient Express":
                        Assert.AreEqual("Train", result.Category);
                        break;
                    case "Ford":
                        Assert.AreEqual("Car", result.Category);
                        break;
                    case "puch":
                        Assert.AreEqual("Bike", result.Category);
                        break;
                }
            }
        }


        [TestMethod]
        public void Classify_Collection_Enum()
        {
            string[] items = ["Super hornet", "Orient Express", "Ford", "puch"];

            foreach (var result in items.Classify<TestCategories>(Model))
            {
                switch (result.Item)
                {
                    case "Super hornet":
                        Assert.AreEqual(TestCategories.Plane, result.Category);
                        break;
                    case "Orient Express":
                        Assert.AreEqual(TestCategories.Train, result.Category);
                        break;
                    case "Ford":
                        Assert.AreEqual(TestCategories.Car, result.Category);
                        break;
                    case "puch":
                        Assert.AreEqual(TestCategories.Bike, result.Category);
                        break;
                }
            }
        }

    }
}