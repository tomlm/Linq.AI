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

    }

}