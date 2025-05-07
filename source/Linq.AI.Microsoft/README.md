# Linq.AI.Microsoft
This library implements the Linq.AI ITransformer for **Microsoft.Extensions.AI** IChatClient. These extensions 
make it easy to manipulate collections of data using LLM AI models.

## Installation
```
dotnet add package Linq.AI.Microsoft
```

## MicrosoftChatClientTransformer
The MicrosoftChatClientTransformer impelemnts the ITransformer around a instance of IChatClient.  

You instantiate a transformer like this:
```csharp
var model = new MicrosoftChatClientTransformer(...instance of IChatClient...);
```
> NOTE: The ChatClient model must support structured output.

# Model methods
The model methods make it easy work to use natural language to process a single item (aka text, object, etc.).

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
The linq extensions use a transformer to process each item in a collections.

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

## Adding Functions
Linq.AI makes it super easy to add functions to your  model. 
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

// create OpenAI Chat Client
var chatClient = new OpenAI.Chat.ChatClient("gpt-4o-mini", config["OpenAIKey"]).AsIChatClient();

// Turn on Function invocation
chatClient = new ChatClientBuilder(chatClient)
    .UseFunctionInvocation();

// Create the transformer and define functions
var model = new MicrosoftChatClientTransformer(chatClient)
    .AddFunctions<MyFunctions>()
    .AddFunction("Sum", "Add 2 numbers", 
        ([Description("the first number to add") int x, 
         [Description("the second number to add") int y) 
        {
            return x+y;
        })
    .AddFunction("LookupAlbumCover", "Lookup up a record album covere", 
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

## V3

* Made all methods Async
* Added IAsyncEnumerable support
* Removed parallel processing, you should use Plinq.NET directly

## V1, V2
* Basic library using parallel internal and non-async processing.

