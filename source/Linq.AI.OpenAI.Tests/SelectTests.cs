using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection.Emit;
using System.Threading;
using System;

namespace Linq.AI.OpenAI.Tests
{

    internal class SourceObject
    {
        public string? Title { get; set; }

        public string? Writer { get; set; }

        public string? Description { get; set; }

        public DateTime? PubliicationDate { get; set; }
    }

    internal class Author
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    internal class TargetObject
    {
        public Author? Author { get; set; }

        public string? AuthorFullName { get; set; }

        public string? Summary { get; set; }
    }



    [TestClass]
    public partial class SelectTests : UnitTestBase
    {
        [TestMethod]
        public async Task Select_Text_String()
        {
            var results = await GetModel().ExtractAsync<string>(Text, "section titles");

            Assert.IsTrue(results.Count() > 1);
            foreach (var result in results)
            {
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public async Task Select_Text_Object()
        {
            var results = await GetModel().ExtractAsync<Article>(Text, "turn each section into an article");

            Assert.IsTrue(results.Count > 1);
            foreach(var result in results)
            {
                Assert.IsNotNull(result.Title);
                Assert.IsNotNull(result.Paragraph);
            }
        }

        [TestMethod]
        public async Task Select_Collection_String2String()
        {
            var sources = new List<string>()
            {
                "The book 'Title 1'\nThis article describes the upcoming battle for the hearts and minds of the republican party. Written by Joe Blow, Oct 1 2020",
                "# Title 2\nThe case is made that Tom Brady is the GOAT QB. A great new story by Suzanne Summers. ",
                "Which is better? Flossing or brushing or doing nothing? Gert Gooble discusses this important subject." ,
            };

            var results = await sources
                                .SelectAsync<string>(GetModel(), "Transform to text like this:\n# {{title}}\nBy {{Author}}\n{{Summary}}")
                                .ToListAsync();
            var result = results[0];
            Assert.IsTrue(result.StartsWith("#"));
            Assert.IsTrue(result.Split('\n').First().Contains("Title 1"));
            Assert.IsTrue(result.Contains("Joe Blow"));

        }

        [TestMethod]
        public async Task Select_Collection_String2Object()
        {
            var sources = new List<string>()
            {
                "The book 'Title 1'\nThis article describes the upcoming battle for the hearts and minds of the republican party. Written by Joe Blow, Oct 1 2020",
                "# Title 2\nThe case is made that Tom Brady is the GOAT QB. A great new story by Suzanne Summers. ",
                "Which is better? Flossing or brushing or doing nothing? Gert Gooble discusses this important subject." ,
            };

            var results = await sources
                .SelectAsync<TargetObject>(GetModel())
                .ToListAsync();
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
        public async Task Select_Collection_Object2Object()
        {
            var sources = new List<SourceObject>()
            {
                new SourceObject() { Title="Title 1", Description="This article describes the upcoming battle for the hearts and minds of the republican party", Writer="Joe Blow", PubliicationDate = new DateTime(2020, 10, 1) },
                new SourceObject() { Title="Title 2", Description="The case is made that Tom Brady is the GOAT QB.", Writer="Suzanne Summers", PubliicationDate = new DateTime(2010, 1, 3) },
                new SourceObject() { Title="Title 3", Description="Which is better? Flossing or brushing or doing nothing?", Writer="Gert Gooble", PubliicationDate = new DateTime(2020, 10, 1) },
            };

            await foreach (var result in sources.SelectAsync<TargetObject>(GetModel()))
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


    }
    public class Article
    {
        public string? Title { get; set; }
        public string? Paragraph { get; set; }
    }


}