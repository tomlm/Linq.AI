using OpenAI.Chat;

namespace Linq.AI.OpenAI.Tests
{

    [TestClass]
    public class TransformationTests : UnitTestBase
    {

        [TestMethod]
        public async Task Transform_String2String()
        {
            var source = "My name is Tom.";
            var transformation = await GetModel().TransformItemAsync<string>(source, "into spanish");
            Assert.IsTrue(await GetModel().CompareAsync("Me llamo Tom.", transformation!));
        }

        [TestMethod]
        public async Task Transform_String2Object()
        {
            var source = "I have 4 children and my name is Inigo Montoya.";
            var obj = await GetModel().TransformItemAsync<TestObject>(source);
            Assert.AreEqual("Inigo Montoya", obj.Name);
            Assert.AreEqual(4, obj.Count);
        }

        [TestMethod]
        public async Task Transform_Object2Object()
        {
            var source = new TestObject2() { FirstName = "Inigo", LastName = "Montoya" };
            var obj = await GetModel().TransformItemAsync<TestObject>(source, instructions: "Do not fill in properties that you don't have data for.");
            Assert.AreEqual("Inigo Montoya", obj.Name);
            Assert.AreEqual(0, obj.Count);
        }

        [TestMethod]
        public async Task Transform_Bool()
        {
            var source = new TestObject2() { FirstName = "Inigo", LastName = "Montoya" };
            Assert.IsTrue(await GetModel().TransformItemAsync<bool>(source, "return true if the <ITEM> has a character in princess bride"));
            Assert.IsFalse(await GetModel().TransformItemAsync<bool>(source, "return true if if the <ITEM> has a character in star wars"));
        }

        [TestMethod]
        public async Task Transform_Classify()
        {
            Assert.AreEqual(TestCategories.Car, await GetModel().TransformItemAsync<TestCategories>("Ford", "classify"));
            Assert.AreEqual(TestCategories.Plane, await GetModel().TransformItemAsync<TestCategories>("Cessna", "classify"));
        }

        [TestMethod]
        public async Task Transform_Classify_text()
        {

            var categories = String.Join(",", Enum.GetNames<TestCategories>());
            Assert.AreEqual("Car", await GetModel().TransformItemAsync<string>("Ford", $"Pick the category from this list: {categories}"));
            Assert.AreEqual("Plane", await GetModel().TransformItemAsync<string>("Cessna", $"Pick the category from this list: {categories}"));
        }

        [TestMethod]
        public async Task Transform_Answer()
        {
            var result = await GetModel().TransformItemAsync<string>(Text, "What town was he born in?");
            Assert.IsTrue(result.ToLower().Contains("honolulu"));
        }

        [TestMethod]
        public async Task Transform_Text2Strings()
        {
            var results = await GetModel().TransformItemAsync<string[]>(Text, "return titles that are bolded");

            string[] titles =
            [
                "Early Life and Education",
                "Law Career and Community Organizing",
                "Political Rise",
                "Presidential Campaign",
                "First Term Achievements",
                "Foreign Policy and Nobel Peace Prize",
                "Second Term Challenges",
                "Post-Presidency Activities",
                "Legacy and Impact",
                "Personal Life"
            ];

            for (int i = 0; i < titles.Length; i++)
            {
                Assert.AreEqual(titles[i].Trim('*'), results[i].Trim('*'));
            }
        }

        [TestMethod]
        public async Task Transform_Text2Objects()
        {
            var results = await GetModel().TransformItemAsync<Article[]>(Text);

            Assert.IsTrue(results.Count() > 0);
            foreach (var result in results)
            {
                Assert.IsNotNull(result.Title);
                Assert.IsNotNull(result.Paragraph);
            }
        }

        [TestMethod]
        public async Task Transform_Collection_String2String()
        {
            var sources = new List<string>()
            {
                "The book 'Title 1'\nThis article describes the upcoming battle for the hearts and minds of the republican party. Written by Joe Blow, Oct 1 2020",
                "# Title 2\nThe case is made that Tom Brady is the GOAT QB. A great new story by Suzanne Summers. ",
                "Which is better? Flossing or brushing or doing nothing? Gert Gooble discusses this important subject." ,
            };

            var results = await GetModel().TransformItemsAsync<string>(sources, "Transform to text like this:\n# {{title}}\nBy {{Author}}\n{{Summary}}").ToListAsync();
            var result = results[0];
            Assert.IsTrue(result.StartsWith("#"));
            Assert.IsTrue(result.Split('\n').First().Contains("Title 1"));
            Assert.IsTrue(result.Contains("Joe Blow"));
            Assert.AreEqual(3, results.Count);
        }

        [TestMethod]
        public async Task Transform_Collection_String2Object()
        {
            var sources = new List<string>()
            {
                "The book 'Title 1'\nThis article describes the upcoming battle for the hearts and minds of the republican party. Written by Joe Blow, Oct 1 2020",
                "# Title 2\nThe case is made that Tom Brady is the GOAT QB. A great new story by Suzanne Summers. ",
                "Which is better? Flossing or brushing or doing nothing? Gert Gooble discusses this important subject." ,
            };

            var results = await GetModel().TransformItemsAsync<TargetObject>(sources).ToListAsync();
            var result = results[0];
            Assert.AreEqual("Joe Blow", result.AuthorFullName);
            Assert.AreEqual("Joe", result.Author!.FirstName);
            Assert.AreEqual("Blow", result.Author.LastName);

            result = results[1];
            Assert.AreEqual("Suzanne Summers", result.AuthorFullName);
            Assert.AreEqual("Suzanne", result.Author!.FirstName);
            Assert.AreEqual("Summers", result.Author.LastName);

            result = results[2];
            Assert.AreEqual("Gert Gooble", result.AuthorFullName);
            Assert.AreEqual("Gert", result.Author!.FirstName);
            Assert.AreEqual("Gooble", result.Author.LastName);

        }

        [TestMethod]
        public async Task Transform_Collection_Object2Object()
        {
            var sources = new List<SourceObject>()
            {
                new SourceObject() { Title="Title 1", Description="This article describes the upcoming battle for the hearts and minds of the republican party", Writer="Joe Blow", PubliicationDate = new DateTime(2020, 10, 1) },
                new SourceObject() { Title="Title 2", Description="The case is made that Tom Brady is the GOAT QB.", Writer="Suzanne Summers", PubliicationDate = new DateTime(2010, 1, 3) },
                new SourceObject() { Title="Title 3", Description="Which is better? Flossing or brushing or doing nothing?", Writer="Gert Gooble", PubliicationDate = new DateTime(2020, 10, 1) },
            };

            await foreach (var result in GetModel().TransformItemsAsync<TargetObject>(sources))
            {
                switch (result.Summary)
                {
                    case "Title 1":
                        Assert.AreEqual("Joe Blow", result.AuthorFullName);
                        Assert.AreEqual("Joe", result.Author!.FirstName);
                        Assert.AreEqual("Blow", result.Author.LastName);
                        break;
                    case "Title 2":
                        Assert.AreEqual("Suzanne Summers", result.AuthorFullName);
                        Assert.AreEqual("Suzanne", result.Author!.FirstName);
                        Assert.AreEqual("Summers", result.Author.LastName);
                        break;
                    case "Title 3":
                        Assert.AreEqual("Gert Gooble", result.AuthorFullName);
                        Assert.AreEqual("Gert", result.Author!.FirstName);
                        Assert.AreEqual("Gooble", result.Author.LastName);
                        break;
                }
            }

        }

        [TestMethod]
        public async Task Transform_Vision_UriTest()
        {
            var uri = new Uri("https://2cupsoftravel.com/wp-content/uploads/2022/10/Oktoberfest-munich-things-to-know.jpg");
            var result = await GetModel().SummarizeAsync(uri);
            Assert.IsTrue(result.Contains("beer"));
        }

        [TestMethod]
        public async Task Transform_Vision_UrisTest()
        {
            var uri = new Uri("https://2cupsoftravel.com/wp-content/uploads/2022/10/Oktoberfest-munich-things-to-know.jpg");
            var uri2 = new Uri("https://2cupsoftravel.com/wp-content/uploads/2022/10/20220928_115250-1200x900.jpg");
            var result = await GetModel().MatchesAsync(new { uri, uri2 }, "Are these pictures of people drinking beer?");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Transform_Vision_Select()
        {
            var uri = new Uri("https://static.vecteezy.com/system/resources/previews/001/222/391/original/blue-elegant-decorative-striped-pattern-editable-text-effect-vector.jpg");
            var results = await GetModel().ExtractAsync<string>(uri, "Extract all phrases from the image");
            Assert.AreEqual("ILLUSTRATOR GRAPHIC STYLE", results[0]);
            Assert.AreEqual("Works with text box or shape", results[1]);
            Assert.AreEqual("Chicago", results[2]);
            Assert.AreEqual("EASY TO USE - 100% EDITABLE", results[3]);
            Assert.AreEqual("Open the graphic style menu and apply", results[4]);
        }

        [TestMethod]
        public async Task Transform_PartTest()
        {
            var uri = new Uri("https://2cupsoftravel.com/wp-content/uploads/2022/10/Oktoberfest-munich-things-to-know.jpg");
            var data = await new HttpClient().GetByteArrayAsync(uri);
            var result = await GetModel().SummarizeAsync(ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(data), "image/jpeg"));
            Assert.IsTrue(result.Contains("beer"));
        }

        [TestMethod]
        public async Task Transform_PartsTest()
        {
            var result = await GetModel().MatchesAsync(new[]
            {
                ChatMessageContentPart.CreateImagePart(new Uri("https://2cupsoftravel.com/wp-content/uploads/2022/10/Oktoberfest-munich-things-to-know.jpg")),
                ChatMessageContentPart.CreateImagePart(new Uri("https://2cupsoftravel.com/wp-content/uploads/2022/10/20220928_115250-1200x900.jpg"))
            }, "Are these pictures of people drinking beer?");
            Assert.IsTrue(result);
        }

        internal class TestObject
        {
            public string? Name { get; set; }

            [System.ComponentModel.Description("The number of children.")]
            public int Count { get; set; } = default;
        }

        internal class TestObject2
        {
            public string? FirstName { get; set; }

            public string? LastName { get; set; }
        }

    }
}