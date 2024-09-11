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
    public class AnswerTests: UnitTestBase
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


        [TestMethod]
        public async Task Answer()
        {
            var answer = await Text.AnswerAsync(Model, "what year was obama born");
            Assert.AreEqual(1961, int.Parse(answer));
        }

        [TestMethod]
        public void Answer_Collection_Objects()
        {
            var results = Forecast.Answer(Model, "What is the temperature difference as an integer?").Select(text => int.Parse(text)).ToList();
            for(int i=0; i < Forecast.Count;i++)
            {
                Assert.AreEqual(Forecast[0].High - Forecast[0].Low, results[0]);
            }
        }

        [TestMethod]
        public void Answer_Collection_Strings()
        {
            var results = Forecast
                .Summarize(Model)
                .Answer(Model, "What is the temperature difference as an integer with no units?").Select(text => int.Parse(text)).ToList();
            for (int i = 0; i < Forecast.Count; i++)
            {
                Assert.AreEqual(Forecast[0].High - Forecast[0].Low, results[0]);
            }
        }

    }
}