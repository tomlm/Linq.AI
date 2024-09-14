#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System.Diagnostics;


namespace Linq.AI.OpenAI.Tests
{
    public class Card
    {
        public string? Title { get; set; }
        public string[]? Bullets { get; set; }
    }

    public class AdaptiveCardLayout
    {
        [System.ComponentModel.Description("AdaptiveCard layout as JSON ")]
        public string? AdaptiveCard { get; set; }
    }

    [TestClass]
    public class Samples : UnitTestBase
    {

        [TestMethod]
        public async Task Matches()
        {
            var goal = """
                Use <TEMPLATE> to transform <ITEM> to a AdaptiveCard JSON
                <TEMPLATE>
                {
                  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                  "type": "AdaptiveCard",
                  "version": "1.5",
                  "body": [
                    {
                       "type": "TextBlock",
                      "text": "{{TITLE}}",
                      "weight": "Bolder",
                      "size": "Medium"
                    },
                    {
                        "type":"TextBlock",
                        "text": "* {{BULLET}}"
                    }
                  ]
                }
                """;
            var cards = (await Text.SelectAsync<Card>(Model, "each section"))
                .Select<string>(Model, goal);

            foreach (var card in cards)
            {
                Debug.WriteLine(card);
            }
        }


        [TestMethod]
        public void Classify()
        {
            string[] artists =
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


            var results = artists
                .Where(name => name.StartsWith("E") || name.StartsWith("Q"))
                .Classify<MusicTypes>(Model);

            Assert.AreEqual(MusicTypes.Pop, results.Single(result => (string)result.Item == "Ed Sheeran").Category);
            Assert.AreEqual(MusicTypes.Rock, results.Single(result => (string)result.Item == "Queen").Category);
        }

        public enum MusicTypes { Pop, Country, Rock, Electronica, HipHop };

        [TestMethod]
        public async Task ArgumentParser()
        {
            var options = await "foo/foo.csproj using Debug configuration for x64 with no logo."
                .TransformItemAsync<CommandLineOptions>(Model);
            Assert.AreEqual("x64", options.Architecture);
            Assert.AreEqual("foo/foo.csproj", options.ProjectOrSolution);
            Assert.AreEqual(true, options.NoLogo);
        }

        private static string GetItemText(object item1, object item2)
        {
            string item1Text = (item1 is string) ? item1 as string : JToken.FromObject(item1).ToString();
            string item2Text = (item2 is string) ? item2 as string : JToken.FromObject(item2).ToString();
            return $$"""
                <ITEM1>
                {{item1Text}}
                
                <ITEM2>
                {{item2Text}}
                """;
        }

        /// <summary>
        /// Compare two objects for semantic equivelence using OpenAI model
        /// </summary>
        /// <param name="source">object to inspect</param>
        /// <param name="transformer">ChatClient for model</param>
        /// <param name="question">question you want answered</param>
        /// <param name="instructions">system instructions to help control the answer</param>
        /// <param name="cancellationToken">cancellation token to cancel the operation.</param>
        /// <returns>answer of question</returns>
        public bool CompareSemantically(object source, object target, ITransformer transformer, string? goal = null, string? instructions = null, CancellationToken cancellationToken = default)
            => GetItemText(source, target).TransformItem<bool>(transformer, goal ?? "are <ITEM1> and <ITEM2> semantically equivelent", instructions, cancellationToken);

        [TestMethod]
        public void Compare_Strings_Semantically()
        {
            Assert.IsTrue(CompareSemantically("fourteen", "14", Model));
            Assert.IsTrue(CompareSemantically("fourteen years old", "10 + 4 years", Model));
            Assert.IsTrue(CompareSemantically("Me llamo Tom", "Mi nombre es Tom", Model));
            Assert.IsTrue(CompareSemantically("My name is Tom", "Mi nombre es Tom", Model, instructions: "allow different langauges to be semantically equal"));
            Assert.IsFalse(CompareSemantically("Me llamo Tom", "Mi padre es Tom", Model));
        }

        [Ignore]
        [TestMethod]
        public async Task Compare_Objects_Semantically()
        {
            var task1= Text.SelectAsync<Article>(Model);
            var task2 = Text.SelectAsync<Article>(Model);
            await Task.WhenAll(task1, task2);
            var models1 = await task1;
            var models2 = await task2;
            for (int i = 0; i < models1.Count(); i++)
            {
                var model1 = models1[i];
                var model2 = models2[i];
                Assert.IsTrue(CompareSemantically(model1, model2, Model));
            }
        }
    }

}