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

            // Create a list of categories formatted as <genre> - <description>
            string[] categories =
            [
                "Pop",
                "Country",
                "Rock",
                "Electronic",
                "Hip Hop"
            ];

            var results = artists.Classify(Model, categories).ToList();

            Assert.AreEqual("Pop", results.Single(result => (string)result.Item == "Ed Sheeran").Category);
            Assert.AreEqual("Rock", results.Single(result => (string)result.Item == "Queen").Category);
        }

        public enum MusicTypes { Pop, Country, Rock, Electronica, HipHop };
    }

    public class Arguments
    {
        [System.ComponentModel.Description("amount of parallezation")]
        public int MaxParallel { get; set; }

        [System.ComponentModel.Description("the project or solution file to operate on. If a file is not specified, the command will search the current directory for one.")]
        public string? Project { get; set; }

        [System.ComponentModel.Description()]
        public bool NoRestore { get; set; }
    }

}