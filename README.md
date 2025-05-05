# Linq.AI
This library adds Linq extension methods which use AI models to transform and manipulate data.

## Installation
```
dotnet add package Linq.AI
dotnet add package Linq.AI.OpenAI
```

## ITransformer 
The Linq.AI package needs an ITransformer for the AI model that is being used. 

Currently there is an implementation for OpenAI which you can get by installing Linq.AI.OpenAI

You instantiate a transformer like this:
```csharp
var model = new OpenAITransformer(model: "gpt-4o-mini", new ApiKeyCredential("<openai.apikey>"));
```
> NOTE: The model must support structured output.

# Model Extensions
The model extensions add methods to the ITransformer model to work with a single item (aka text, object, etc.).

| Extension | Description | 
| ----------| ------------|
| ***.ClassifyAsync()*** | classify the text using a model. |
| ***.SummarizeAsync()*** | Create a summarization for the text by using a model. |
| ***.MatchesAsync()*** | Return whether the text matches using a model. |
| ***.QueryAsync()*** | get the answer to a global question using a model. |
| ***.QueryAboutAsync()*** | get the answer to a question from the text using a model. |
| ***.ExtractAsync()*** | Extract items from the text using a model. |
| ***.CompareAsync()*** | Compare 2 objects for semantic equivelancy |

## model.ClassifyAsync() 
Classify an item using an enumeration or list of categories.

```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classification = await model.ClassifyAsync<Genres>(item);
```

You can use a collection of strings as categories as well.
```csharp
var classification = await model.ClassifyAsync<string>(item, new [] {"Rock","Pop","Electronica","Country","Classical"});
```

## model.SummarizeAsync() 
Summarize the item 

```csharp
var summary = await model.SummarizeAsync(item, "with 3 words");
```

## model.MatchesAsync() 
Get true/false if the item matches the question

```csharp
if (await model.MatchesAsync(item, "there is date"))
  ...
```

## model.QueryAsync() 
Ask a question using models knowledge.

```csharp
var answer = await model.QueryAsync("what is the birthday of barack obama?");
```

Query can take a typed output
```csharp
var date = await model.QueryAsync<int>("what is the day of the month for the birthday of barack obama?");
```


## model.QueryAboutAsync() 
Ask a question about an item

```csharp
var answer = await model.QueryAboutAsync(item, "what is his birthday?");
```

It also can take a typed output
```csharp
var date = await model.QueryAboutAsync<int>(person, "how tall is he in inches?");
```

## model.ExtractAsync() 
Extracts a collection of items from the item.

Example using model to select text from the item
```csharp
var words = await model.ExtractAsync<string>(item, "The second word of every paragraph");
```

You can use the model to select structured data.
```csharp
public class HREF 
{ 
	public string Url {get;set;}
	public string Title {get;set;}
}

var hrefs = await model.ExtractAsync<HREF>(item).ToListAsync();
```

## model.Compare()
Compares two objects for semantic equivelancy.

```csharp
Assert.IsTrue(await Model.CompareAsync("fourteen", "14"));
Assert.IsTrue(await Model.CompareAsync("fourteen years old", "10 + 4 years"));
Assert.IsTrue(await Model.CompareAsync("Me llamo Tom", "Mi nombre es Tom"));
Assert.IsTrue(await Model.CompareAsync("My name is Tom", "Mi nombre es Tom", instructions: "allow different langauges to be semantically equal"));
Assert.IsFalse(await Model.CompareAsync("Me llamo Tom", "Mi padre es Tom"));
Assert.IsTrue(await Model.CompareAsync(
	new 
	{ 
	    Introduction = "My name is Tom", 
	    Background="I live in Kirkland, Washington"
	},
	new
	{
	    Introduction = "I'm Tom",
	    Background = "I'm from Kirkland, Washington"
	}));
```

# Linq Extensions 
The object extensions use the ITransformer model to work each item in a collections.

| Extension | Description | 
| ----------| ------------|
| ***.SelectAsync()*** | Transforms each item into another format with natural language. |
| ***.WhereAsync()*** | Keeps each item which matches with natural language filter. |
| ***.ClassifyAsync()*** | Classify each item. |
| ***.SummarizeAsync()*** | Create a summarization for each item. |
| ***.QueryAboutEachAsync()*** | Gets the question to a question about each item. |

## enumerable.SelectAsync() 
.SelectAsync() let's you transform the source into target using an ITransformer model.

You can use it to transform an object from one format to another by simply giving the types. The model
will use AI to appropriately map the properties between the object types.
```csharp
var targetItems = await items.SelectAsync<SourceItem,TargetItem>(model).ToListAsync();
```

Transform a collection of text into another format, like markdown.
```csharp
var markdownItems = awit items.SelectAsync(model,	goal: """"
					transform each item into markdown like this:
					# {{TITLE}}
					{{AUTHOR}}
					{{DESCRIPTION}}
					""");
```

## enumerable.WhereAsync()
Filter a collection using natural language
```csharp
var smallItems = await items.WhereAsync(model, "item would fit in a bread box");
```


## enumerable.ClassifyAsync() 
This allows you to classify each item using a model;
```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classifiedItems = await items.ClassifyAsync<Genres>(model);
```


## enumerable.SummarizeAsync() 
Generate text summary for each item using an ITransformer model.

```chsarp
var summaries = await items.SummarizeAsync(model);
```

You can control the summarization with a hint
```csharp
var summaries = await items.SummarizeAsync(model, "generate a 3 word summary");
```

## enumerable.QueryAboutEach() 
This operator let's you ask a question for each item in a collection.
```csharp
var answers = await items.QueryAboutEachAsync<float>(model, "What is the cost?");
```

# ITransformer 
The ITransformer implements the core primatives for using AI to manipulate generation and transformation.

| Extension | Description | 
| ----------| ------------|
| ***.GenerateAsync()*** | use a model and a goal to return a shaped result. |
| ***.TransformItemAsync()*** | use a model and a goal to transform an item into a shaped result. |
| ***.TransformItemsAsync()*** | use a model and a goal to transform a collection of items into a collection of shaped results. |

## transformer.GenerateAsync()
Given a model and a goal return a shaped result.
```csharp
var haiku = await transformer.GenerateAsync<string>("write a haiku about camping");
var names = await transformer.GenerateAsync<string[]>("funny names for people named bob");
var cities = await transformer.GenerateAsync<City[]>("return the top 5 largest cities in the world.");
```

## transformer.TransformItemAsync()
Given a model and a goal return a shaped result.
```csharp
var result = await model.TransformItemAsync<string>("my name is Tom", "translate to spanish);
// ==> "Me llamo Tom"
```

## transformer.TransformItemsAsync()
Transform a collection of items using a model and a goal.
```csharp
var items = new string[] {"Hello", "My name is Tom", "One more please"];
var results = await items.TransformItemsAsync(model, "translate to spanish);
// result[0] = "Hola"
// result[1] = "Me llamo Tom"
// result[2] = "Una mas, por favor"
```

## Linq.AI and Vision
You can use Linq.AI and the model to work with images.  

Simply pass a **Uri()** to an image or a **ChatMessageContentPart** for the image and 
you can call any of the extension methods.

Examples:
```csharp
var uri = new Uri("https://2cupsoftravel.com/wp-content/uploads/2022/10/Oktoberfest-munich-things-to-know.jpg");
var uri2 = new Uri("https://2cupsoftravel.com/wp-content/uploads/2022/10/20220928_115250-1200x900.jpg");

// summarize an image uri
var summmary = await Model.SummarizeAsync(uri);

// ask a question about a group of image uris
var matches = await Model.MatchesAsync(new [] { uri, uri2 }, "Are these pictures of people drinking beer?");

// extract text out of an image uri.
var text = await Model.SelectAsync<string>(uri, "Extract all phrases from the image");

// upload and classify an image binary
var imagePart = ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), "image/jpeg");
var classification = await Model.ClassifyAsync<Mood>(imagePart);
```

## Adding Tools
Linq.AI makes it super easy to add tools to your OpenAI model. 
There are 2 ways to do it:
* Define a static class with static methods on it
* Create a Delegate 

If the model decides it needs the result of your function Liqn.AI will automatically invoke the function doing all of the 
type wrangling needed and then pass the result back to the model to use to create the final answer.

> NOTE: Delegates can be synchronous or async.
 
```csharp
public static class MyFunctions
{
    [Description("Perform PowPow calculation")]
    public static double PowPow(  [Description("first number")][Required] double x, 
                                  [Description("second number")][Required] double y)
    {
        return Math.Pow(x,y);
    }
}

var model = new new OpenAITransformer(model: "gpt-4o-mini", "<open ai key>")
    .AddTools<MyFunctions>()
    .AddTool("Sum", "Add 2 numbers", 
        ([Description("the first number to add") int x, 
         [Description("the second number to add") int y) 
        {
            return x+y;
        })
    .AddTool("LookupAlbumCover", "Lookup up a record album covere", 
        async ([Description("the album name") string name, CancellationToken ct) => 
        {
            await Task.Delay(1000);
            return result;
        });
```

Now if query needs the result of the function it will just work
```csharp
    await model.GenerateAsync<double>("What's the powpow for 3 and 4?");
```

# InstructionAttribute
You can add [Description] or [Instruction] attributes to properties on you classes to help the LLM properly work with your objects when
the property name is amibiguious.

```csharp
public class LeaderInfo
{
    [Instruction("The name of country that the person is leader for")]
    public string Name {get;set;}

    [Instruction("The title for the leadership role the leader has. (Example: President)")]
    public string Title {get;set;}

    [Instruction("The full name for the leader")]
    public string FullName {get;set;}

    [Instruction("The year they took the role.")]
    public int? Date { get; set;}
}
var leader = await model.QueryAsync<LeaderInfo>("barack obama");
// {"Name":"United States","Title":"President","FullName":"Barack Hussein Obama II","Date":2009}
```

> This library was heaviy inspired by stevenic's [agentm-js](https://github.com/stevenic/agentm-js) library, Kudos!

# Changes
## V2
* Made all methods Async
* Added IAsyncEnumerable support
* Removed parallel processing, you should use Plinq.NET directly

## V1
* Basic library

