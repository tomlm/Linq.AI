using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel.DataAnnotations;

namespace Linq.AI.OpenAI.Tests
{
    public enum TemperatureUnits { Celsius, Farenheit };

    public class MyFunctions
    {
        [Instruction("Perform magic calculation")]
        public static double MagicMath([System.ComponentModel.Description("first number")][Required] double x, [System.ComponentModel.Description("second number")][Required] double y)
            => Math.Pow(x, y);


        [Instruction("Increment the Item Counter ")]
        public static TestItem IncrementItemCounter(CompletionContext context)
        {
            var item = context.Item as TestItem;
            item.Counter++;
            return item;
        }

        [System.ComponentModel.Description("Lookup weather for a location")]
        public static async Task<WeatherReport> GetWeatherForLocation([Required] string location, TemperatureUnits? unit, CancellationToken ct = default)
        {
            await Task.Delay(100);
            if (unit == null || unit == TemperatureUnits.Farenheit)
            {
                return new WeatherReport()
                {
                    Location = location,
                    Temperature = 100.0f,
                    Unit = TemperatureUnits.Farenheit
                };
            }
            else
            {
                return new WeatherReport()
                {
                    Location = location,
                    Temperature = 37.0f,
                    Unit = TemperatureUnits.Celsius
                };
            }
        }
    }

    public class TestContact
    {
        public required string Name { get; set; }
        public required string HomeTown { get; set; }
    }

    public class WeatherReport
    {
        public required string Location { get; set; }
        public required float Temperature { get; set; }
        public required TemperatureUnits Unit { get; set; }
    }

    public class UnitTestBase
    {
        public virtual ITransformer GetModel()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<ClassifyTests>()
                .Build();
            return new OpenAITransformer(model: "gpt-4o-mini", new ApiKeyCredential(config["OpenAIKey"]));
        }

        public string Text = """
                ### Barack Obama: A Journey of Hope and Change

                **Early Life and Education**  
                Barack Hussein Obama II was born on August 4, 1961, in Honolulu, Hawaii. His father, Barack Obama Sr., was from Kenya, and his mother, Ann Dunham, was from Kansas. Growing up in a multicultural environment, Obama faced the challenges of understanding his identity. He attended Punahou School, an elite private school in Hawaii, and later moved to Los Angeles to attend Occidental College. He then transferred to Columbia University in New York City, where he graduated with a degree in political science in 1983.

                **Law Career and Community Organizing**  
                After college, Obama moved to Chicago, where he worked as a community organizer in the city's South Side. His work focused on improving living conditions in poor neighborhoods. In 1988, he enrolled at Harvard Law School, where he became the first African American president of the prestigious Harvard Law Review. After graduating magna cum laude in 1991, he returned to Chicago to practice civil rights law and teach constitutional law at the University of Chicago.

                **Political Rise**  
                Obama's political career began in the Illinois State Senate, where he served from 1997 to 2004. During his tenure, he worked on legislation to expand healthcare and early childhood education programs for the poor. His keynote address at the 2004 Democratic National Convention catapulted him into the national spotlight, leading to his election to the U.S. Senate later that year.

                **Presidential Campaign**  
                In 2008, Obama announced his candidacy for the presidency. His campaign slogans, "Change We Can Believe In" and "Yes We Can," resonated with a wide range of voters. He won the Democratic nomination after a hard-fought primary battle against Hillary Clinton and went on to defeat Republican candidate John McCain in the general election, becoming the first African American president of the United States.

                **First Term Achievements**  
                Obama's first term was marked by significant legislative achievements, including the passage of the Affordable Care Act (ACA), commonly known as Obamacare. The ACA aimed to reduce healthcare costs and expand insurance coverage. His administration also passed the Dodd-Frank Wall Street Reform and Consumer Protection Act to prevent the recurrence of the financial crisis.

                **Foreign Policy and Nobel Peace Prize**  
                In 2009, Obama was awarded the Nobel Peace Prize for his efforts to strengthen international diplomacy and cooperation. His foreign policy initiatives included the withdrawal of U.S. troops from Iraq, the killing of Osama bin Laden in 2011, and the signing of the Iran Nuclear Deal in 2015. These actions aimed to promote global stability and security.

                **Second Term Challenges**  
                Obama's second term faced numerous challenges, including political gridlock and rising partisan tensions. Despite these obstacles, his administration achieved significant milestones, such as the legalization of same-sex marriage by the Supreme Court in 2015 and the implementation of the Deferred Action for Childhood Arrivals (DACA) program, which provided temporary relief from deportation for undocumented immigrants brought to the U.S. as children.

                **Post-Presidency Activities**  
                After leaving office in 2017, Obama continued to be an influential figure in American politics and global affairs. He and his wife, Michelle Obama, founded the Obama Foundation, which focuses on leadership development and civic engagement. He has also written several books, including his memoir "A Promised Land," which provides an in-depth look at his presidency.

                **Legacy and Impact**  
                Obama's presidency is often remembered for its historic significance and the hope it inspired in millions of people. His leadership style emphasized inclusivity, diplomacy, and progressive change. While his policies and decisions were sometimes met with criticism, his impact on American society and politics remains profound.

                **Personal Life**  
                Barack Obama is married to Michelle Obama, a lawyer and former First Lady of the United States. They have two daughters, Malia and Sasha. The Obamas are known for their strong family values and commitment to public service. They continue to inspire people around the world through their work and advocacy.
                
                """;

    }
}
