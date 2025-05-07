
namespace Linq.AI.Microsoft.Tests
{

    public enum TestCategories { Car, Bike, Train, Plane };

    [TestClass]
    public class ClassifyTests : UnitTestBase
    {
        public static string[] Categories = ["Car", "Bike", "Train", "Plane"];

        [TestMethod]
        public async Task Classify_Text_Enum()
        {
            var result = await GetModel().ClassifyAsync<TestCategories>("Ford");
            Assert.AreEqual(TestCategories.Car, result);
        }

        [TestMethod]
        public async Task Classifiy_Text_Strings()
        {
            var result = await GetModel().ClassifyAsync("Ford", Categories);
            Assert.AreEqual("Car", result);
        }

        [TestMethod]
        public async Task Classify_Collection_Strings()
        {
            string[] items = ["Cessna", "Orient Express", "Ford", "Trek"];

            await foreach (var result in items.ClassifyAsync(GetModel(), Categories))
            {
                switch (result.Item)
                {
                    case "Cessna":
                        Assert.AreEqual("Plane", result.Category);
                        break;
                    case "Orient Express":
                        Assert.AreEqual("Train", result.Category);
                        break;
                    case "Ford":
                        Assert.AreEqual("Car", result.Category);
                        break;
                    case "Trek":
                        Assert.AreEqual("Bike", result.Category);
                        break;
                }
            }
        }

        [TestMethod]
        public async Task Classify_Collection_Objects()
        {
            string[] items = ["Cessna", "Orient Express", "nash", "Trek"];

            await foreach (var result in items
                            .Select(name => new TestObject() { Name = name })
                            .ClassifyAsync(GetModel(), Categories))
            {
                switch (result.Item.Name)
                {
                    case "Cessna":
                        Assert.AreEqual("Plane", result.Category);
                        break;
                    case "Orient Express":
                        Assert.AreEqual("Train", result.Category);
                        break;
                    case "Ford":
                        Assert.AreEqual("Car", result.Category);
                        break;
                    case "Trek":
                        Assert.AreEqual("Bike", result.Category);
                        break;
                }
            }

        }


        [TestMethod]
        public async Task Classify_Collection_Enum()
        {
            string[] items = ["Cessna", "Orient Express", "Ford", "Trek"];

            await foreach (var result in items
                                        .ClassifyAsync<TestCategories>(GetModel()))
            {
                switch (result.Item)
                {
                    case "Cessna":
                        Assert.AreEqual(TestCategories.Plane, result.Category);
                        break;
                    case "Orient Express":
                        Assert.AreEqual(TestCategories.Train, result.Category);
                        break;
                    case "Ford":
                        Assert.AreEqual(TestCategories.Car, result.Category);
                        break;
                    case "Trek":
                        Assert.AreEqual(TestCategories.Bike, result.Category);
                        break;
                }
            }

        }

        [TestMethod]
        public async Task Classify_Collection_Enum_Parralel()
        {
            string[] items = ["Cessna", "Orient Express", "Ford", "Trek"];

            await foreach (var result in items.ClassifyAsync<TestCategories>(GetModel()))
            {
                switch (result.Item)
                {
                    case "Cessna":
                        Assert.AreEqual(TestCategories.Plane, result.Category);
                        break;
                    case "Orient Express":
                        Assert.AreEqual(TestCategories.Train, result.Category);
                        break;
                    case "Ford":
                        Assert.AreEqual(TestCategories.Car, result.Category);
                        break;
                    case "Trek":
                        Assert.AreEqual(TestCategories.Bike, result.Category);
                        break;
                }
            }

        }

    }
}