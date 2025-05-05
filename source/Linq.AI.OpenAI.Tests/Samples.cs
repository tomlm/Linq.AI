#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System.Diagnostics;
using System.Reflection;


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
            var cards = (await GetModel().ExtractAsync<Card>(Text, "each section"))
                    .SelectAsync<string>(GetModel(), goal);

            await foreach (var card in cards)
            {
                Debug.WriteLine(card);
            }
        }


        [TestMethod]
        public async Task Classify()
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


            var results = await artists
                .Where(name => name.StartsWith("E") || name.StartsWith("Q"))
                .ClassifyAsync<MusicTypes>(GetModel())
                .ToListAsync();

            Assert.AreEqual(MusicTypes.Pop, results.Single(result => (string)result.Item == "Ed Sheeran").Category);
            Assert.AreEqual(MusicTypes.Rock, results.Single(result => (string)result.Item == "Queen").Category);
        }

        public enum MusicTypes { Pop, Country, Rock, Electronica, HipHop };

        [TestMethod]
        public async Task ArgumentParser()
        {
            var options = await GetModel().TransformItemAsync<CommandLineOptions>("foo/foo.csproj using Debug configuration for x64 with no logo.");
            Assert.AreEqual("x64", options.Architecture);
            Assert.AreEqual("foo/foo.csproj", options.ProjectOrSolution);
            Assert.AreEqual(true, options.NoLogo);
        }

        public class PresidentInfo
        {
            [Instruction("Name of the president")]
            public string President { get; set; }

            public bool IsAlive { get; set; }

            [Instruction("The president's age")]
            public int? Age { get; set; }
        }


        [TestMethod]
        public async Task TestComplex()
        {
            var results = new List<PresidentInfo[]>()
            {
                await GetModel().GenerateAsync<PresidentInfo[]>("complete list of all presidents of the united states"),
                await GetModel().GenerateAsync<PresidentInfo[]>("complete list of all presidents of the united states")
            };
            
            var result = GetModel().CompareAsync(results[0], results[1]);
        }
    }

}