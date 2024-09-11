namespace Linq.AI.OpenAI.Tests
{

    public enum Categories { Car, Bike, Train, Plane };
    
    [TestClass]
    public class ClassifyTests : UnitTestBase
    {
        [TestMethod]
        public void Classify_Strings()
        {
            string[] items = ["Super hornet", "Orient Express", "Ford", "puch"];
            string[] categories = ["Car", "Bike", "Train", "Plane"];

            foreach (var result in items.Classify(ChatClient, categories))
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
        public void Classify_Objects()
        {
            string[] items = ["Super hornet", "Orient Express", "nash", "puch"];
            string[] categories = ["Car", "Bike", "Train", "Plane"];

            foreach (var result in items.Select(name => new TestObject() { Item = name }).Classify(ChatClient, categories))
            {
                switch (result.Item.Item)
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
        public void Classify_Enum()
        {
            string[] items = ["Super hornet", "Orient Express", "Ford", "puch"];

            foreach (var result in items.Classify<Categories>(ChatClient))
            {
                switch (result.Item)
                {
                    case "Super hornet":
                        Assert.AreEqual(Categories.Plane, result.Category);
                        break;
                    case "Orient Express":
                        Assert.AreEqual(Categories.Train, result.Category);
                        break;
                    case "Ford":
                        Assert.AreEqual(Categories.Car, result.Category);
                        break;
                    case "puch":
                        Assert.AreEqual(Categories.Bike, result.Category);
                        break;
                }
            }
        }

        [TestMethod]
        public void Classify_Sample()
        {
            string[] artists= 
            [
                "Martin Garrix",
                "Carrie Underwood",
                "Queen",
                "Ariana Grande",
                "Dua Lipa",
                "Foo Fighters",
                "Calvin Harris",
                "Marshmello",
                "Nicki Minaj",
                "Justin Bieber",
                "Led Zeppelin",
                "Blake Shelton",
                "Luke Combs",
                "The Rolling Stones",
                "David Guetta",
                "The Beatles",
                "Taylor Swift",
                "Kendrick Lamar",
                "Eminem",
                "Drake",
                "Dolly Parton",
                "Tiesto",
                "Travis Scott",
                "Ed Sheeran",
                "Miranda Lambert"
            ];

            // Create a list of categories formatted as <genre> - <description>
            string[] categories = 
            [
                "Pop - Popular music that appeals to a wide audience",
                "Country - Music that originated in the southern United States",
                "Rock - Music characterized by a strong beat and amplified instruments",
                "Electronic - Music produced using electronic devices",
                "Hip Hop - Music that features rap and a strong rhythmic beat"
            ];

            var results = artists.Classify(ChatClient, categories, "Identify the genre of each artist from the list.").ToList();

            Assert.AreEqual("Pop", results.Single(result => result.Item == "Ed Sheeran").Category);
            Assert.AreEqual("Rock", results.Single(result => result.Item == "Queen").Category);
        }

    }
}