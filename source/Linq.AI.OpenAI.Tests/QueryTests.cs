#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Newtonsoft.Json.Linq;

namespace Linq.AI.OpenAI.Tests
{

    public class Weather
    {
        public string Date { get; set; }
        public int High { get; set; }
        public int Low { get; set; }
        public string Description { get; set; }
    }


    [TestClass]
    public class QueryAboutTests: UnitTestBase
    {
        public List<Weather> Forecast = JArray.Parse("""
            [
                {
                    "date": "2024-09-03",
                    "high": 76,
                    "low": 62,
                    "description": "Sunny"
                },
                {
                    "date": "2024-09-04",
                    "high": 74,
                    "low": 60,
                    "description": "Partly Cloudy"
                },
                {
                    "date": "2024-09-05",
                    "high": 72,
                    "low": 58,
                    "description": "Mostly Sunny"
                },
                {
                    "date": "2024-09-06",
                    "high": 70,
                    "low": 56,
                    "description": "Cloudy"
                },
                {
                    "date": "2024-09-07",
                    "high": 68,
                    "low": 55,
                    "description": "Rain"
                },
                {
                    "date": "2024-09-08",
                    "high": 67,
                    "low": 54,
                    "description": "Showers"
                },
                {
                    "date": "2024-09-09",
                    "high": 69,
                    "low": 55,
                    "description": "Partly Cloudy"
                }
            ]
            """).ToObject<List<Weather>>()!;


        public class LeaderInfo
        {
            [Instruction("The name of country that the person is leader for")]
            public string Name { get; set; }

            [Instruction("The title for the leadership role the leader has. (Example: President)")]
            public string Title { get; set; }

            [Instruction("The full name for the leader")]
            public string FullName { get; set; }

            [Instruction("The year they took the role.")]
            public int? Date { get; set; }
        }
        [TestMethod]
        public void Query()
        {

            var leader = GetModel().Query<LeaderInfo>("Barack Obama");
            Assert.AreEqual(2009, leader.Date);
            Assert.AreEqual("President", leader.Title);
        }

        [TestMethod]
        public async Task QueryAsync()
        {
            var leader = await GetModel().QueryAsync<LeaderInfo>("Barack Obama");
            Assert.AreEqual(2009, leader.Date);
            Assert.AreEqual("President", leader.Title);
        }

        [TestMethod]
        public void QueryAbout()
        {
            var answer = GetModel().QueryAbout(Text, "what is the name of the city where was obama born");
            Assert.AreEqual("honolulu", answer.ToLower());
        }

        [TestMethod]
        public void Query_Collection_Objects()
        {
            var results = Forecast.QueryAboutEach<int>(GetModel(), "What is the temperature difference as an integer?");
            for(int i=0; i < Forecast.Count;i++)
            {
                Assert.AreEqual(Forecast[0].High - Forecast[0].Low, results[0]);
            }
        }

        [TestMethod]
        public void QueryAbout_Collection_Strings()
        {
            var results = Forecast
                .Summarize(GetModel())
                .QueryAboutEach<int>(GetModel(), "What is the temperature difference as an integer with no units?");
            for (int i = 0; i < Forecast.Count; i++)
            {
                Assert.AreEqual(Forecast[0].High - Forecast[0].Low, results[0]);
            }
        }

    }
}