# Linq.AI
This library adds Linq extension methods which use AI models to transform and manipulate data.

> This library was heaviy inspired by stevenic's [agentm-js](https://github.com/stevenic/agentm-js) library, Kudos!

## Installation
```
dotnet add package Linq.AI
dotnet add package Linq.AI.OpenAI
```

## ITransformer 
The Linq.AI package needs an ITransformer for the AI model that is being used. 

Currently there is an implementation for OpenAI which you can get by installing Linq.AI.OpenAI

And then instantiate a transformer like this:
```csharp
var model = new new OpenAITransformer(model: "gpt-4o-mini", "<open ai key>");
```
> NOTE: The model must support structured output.

# Model Extensions
The model extensions use the ITransformer model to work with a single item (aka text, object, etc.).

| Extension | Description | 
| ----------| ------------|
| ***.Classify()/.ClassifyAsync()*** | classify the text using a model. |
| ***.Summarize()/.SummarizeAsync()*** | Create a summarization for the text by using a model. |
| ***.Matches()/.MatchesAsync()*** | Return whether the text matches using a model. |
| ***.Query()/.QueryAsync()*** | get the answer to a global question using a model. |
| ***.QueryAbout()/.QueryAboutAsync()*** | get the answer to a question from the text using a model. |
| ***.Select()/.SelectAsync()*** | Select a collection of items from the text using a model. |
| ***.Compare()/.CompareAsync()*** | Compare 2 objects for semantic equivelancy |

## model.Classify() 
Classify an item using an enumeration or list of categories.

```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classification = model.Classify<Genres>(item);
```

## model.Summarize() 
Summarize the item 

```csharp
var summary = model.Summarize(item, "with 3 words");
```

## model.Matches() 
Get true/false if the item matches the question
```csharp
if (model.Matches(item, "there is date"))
  ...
```

## model.Query() 
Ask a question using models knowledge.

```csharp
var answer = model.Query(item, "what is the birthday of barack obama?");
```

## model.QueryAbout() 
Ask a question about an item

```csharp
var answer = model.QueryAbout(item, "what is his birthday?");
```

## model.Select() 
Extracts a collection of items from the item.

Example using model to select text from the item
```csharp
var words = model.Select<string>(item, "The second word of every paragraph");
```

Example using model to select structed data.
```csharp
public class HREF 
{ 
	public string Url {get;set;}
	public string Title {get;set;}
}
var summary = model.Select<HREF>(item);
```

## model.Compare()
Compares two objects for semantic equivelancy.

```csharp
Assert.IsTrue(Model.Compare("fourteen", "14"));
Assert.IsTrue(Model.Compare("fourteen years old", "10 + 4 years"));
Assert.IsTrue(Model.Compare("Me llamo Tom", "Mi nombre es Tom"));
Assert.IsTrue(Model.Compare("My name is Tom", "Mi nombre es Tom", instructions: "allow different langauges to be semantically equal"));
Assert.IsFalse(Model.Compare("Me llamo Tom", "Mi padre es Tom"));
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
| ***.Select()*** | Transforms each item into another format with natural language. |
| ***.Where()*** | Keeps each item which matches with natural language filter. |
| ***.Remove()*** | Remove each item which matches a natural language filter. |
| ***.Classify()*** | Classify each item. |
| ***.Summarize()*** | Create a summarization for each item. |
| ***.QueryAboutEach()*** | Gets the question to a question about each item. |

> NOTE: These methods internally run AI calls as throttled parallel background tasks.

## enumerable.Select() 
.Select() let's you transform the source into target using an ITransformer model.

You can use it to transform an object from one format to another by simply giving the types. The model
will use AI to appropriately map the properties between the object types.
```csharp
var targetItems = items.Select<SourceItem,TargetItem>(model)
```

Transform a collection of text into another format, like markdown.
```csharp
var markdownItems = items.Select(model,	goal: """"
					transform each item into markdown like this:
					# {{TITLE}}
					{{AUTHOR}}
					{{DESCRIPTION}}
					""");
```

## enumerable.Where()/enumerable.Remove() 
Filter a collection using natural language
```csharp
var smallItems = items.Where(model, "item would fit in a bread box");
var bigItems = items.Remove(model, "item would fit in a bread box");
```


## enumerable.Classify() 
This allows you to classify each item using a model;
```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classifiedItems = items.Classify<Genres>(model);
```


## enumerable.Summarize() 
Generate text summary for each item using an ITransformer model.

```chsarp
var summaries = items.Summarize(model);
```

You can control the summarization with a hint
```csharp
var summaries = items.Summarize(model, "generate a 3 word summary");
```

## enumerable.QueryAboutEach() 
This operator let's you ask a question for each item in a collection.
```csharp
var answers = items.QueryAboutEach<float>(model, "What is the cost?");
```

# ITransformer 
The ITransformer implements the core primatives for using AI to manipulate generation and transformation.

| Extension | Description | 
| ----------| ------------|
| ***.Generate()/.GenerateAsync()*** | use a model and a goal to return a shaped result. |
| ***.TransformItem()/.TransformItemAsync()*** | use a model and a goal to transform an item into a shaped result. |
| ***.TransformItems()*** | use a model and a goal to transform a collection of items into a collection of shaped results. |

## transformer.Generate()
Given a model and a goal return a shaped result.
```csharp
var haiku = transformer.Generate<string>("write a haiku about camping");
var names = transformer.Generate<string[]>("funny names for people named bob");
var cities = transformer.Generate<City[]>("return the top 5 largest cities in the world.");
```

## transformer.TransformItem()
Given a model and a goal return a shaped result.
```csharp
var result = model.TransformItem<string>("my name is Tom", "translate to spanish);
// ==> "Me llamo Tom"
```

## transformer.TransformItems()
Transform a collection of items using a model and a goal.
```csharp
var items = new string[] {"Hello", "My name is Tom", "One more please"];
var results = items.TransformItems(model, "translate to spanish);
// result[0] = "Hola"
// result[1] = "Me llamo Tom"
// result[2] = "Una mas, por favor"
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
var leader = model.Query<LeaderInfo>("barack obama");
// {"Name":"United States","Title":"President","FullName":"Barack Hussein Obama II","Date":2009}
```

# Defining new operators
To create a custom operator you create an static class and define static methods for the ITransformer or collection of objects.

For example, here is the implementation of Summarize():

* The ***SummarizeAsync()*** method defines object operator which calls **TransformItemAsync** with a default goal of "Create a summarization" with result type of string.
* The ***Summarize()*** method defines a collection operator which calls **TransformItems** with a default goal of "Create a summarization" with result type for each item in the collection of string.

```csharp
    public static class SummarizeExtension
    {
        // operator to summarize object
        public static string Summarize(this ITransformer model, object item, string? goal, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItem<string>(item, goal ?? "create a summarization", instructions, cancellationToken);

        // operator to summarize object
        public static Task<string> SummarizeAsync(this ITransformer model, object item, string? goal, string? instructions = null, CancellationToken cancellationToken = default)
            => model.TransformItemAsync<string>(item, goal ?? "create a summarization", instructions, cancellationToken);

        // operator to summarize collection of objects
        public static IList<string> Summarize(this IEnumerable<object> source, ITransformer model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => model.TransformItems<string>(source, goal ?? "create a summarization", instructions, maxParallel, cancellationToken);
    }
```
