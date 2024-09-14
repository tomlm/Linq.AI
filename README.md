# Linq.AI
This library adds Linq extension methods which use AI models to transform and manipulate data.

> This library was heaviy inspired by stevenic's [agentm-js](https://github.com/stevenic/agentm-js) library, Kudos!

## Installation
```dotnet add package Linq.AI```

## ITransformer 
The Linq.AI package needs an ITransformer for the AI model that is being used. 

Currently there is an implementation for OpenAI which you can get by installing Linq.AI.OpenAI:

```dotnet add package Linq.AI.OpenAI ```

And then instantiate a transformer like this:
```csharp
var model = new new OpenAITransformer(model: "gpt-4o-mini", "<open ai key>");
```
> NOTE: The model must support structured output.

# Object Extensions
The object extensions use the ITransformer model to work with a single item (aka text, object, etc.).

| Extension | Description | 
| ----------| ------------|
| ***.Classify()/.ClassifyAsync()*** | classify the text using a model. |
| ***.Summarize()/.SummarizeAsync()*** | Create a summarization for the text by using a model. |
| ***.Matches()/.MatchesAsync()*** | Return whether the text matches using a model. |
| ***.Answer()/.AnswerAsync()*** | get the answer to a question from the text using a model. |
| ***.Select()/.SelectAsync()*** | Select a collection of items from the text using a model. |

## item.Classify() 

```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classification = item.Classify<Genres>(model);
```

## item.Summarize() 

```csharp
var summary = item.Summarize(model, "with 3 words");
```

## item.Matches() 

```csharp
if (item.Matches(model, "there is date"))
  ...
```

## item.Answer() 

```csharp
var summary = text.Answer(model, "what is the birthday?");
```

## item.Select() 
Select pulls a collection of items from the source.

Example using model to select text from the item
```csharp
var words = item.Select<string>(model, "The second word of every paragraph");
```

Example using model to select structed data.
```csharp
public class HREF 
{ 
	public string Url {get;set;}
	public string Title {get;set;}
}
var summary = text.Select<HREF>(model);
```

# Linq Extensions 
The object extensions use the ITransformer model to work each item in a collections.

| Extension | Description | 
| ----------| ------------|
| ***.Select()*** | Transforms each item into another format with natural language. |
| ***.Where()*** | Keeps each item which matches with natural language filter. |
| ***.Remove()*** | Remove each item which matches a natural language filter. |
| ***.Summarize()*** | Create a summarization for each item. |
| ***.Classify()*** | Classify each item. |
| ***.Answer()*** | Answers the question for each item. |

> NOTE: These methods internally run AI calls as throttled parallel background tasks.

## enumerable.Classify() 
This allows you to classify each item using a model;
```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classifiedItems = items.Classify<Genres>(model);
```

## enumerable.Where()/enumerable.Remove() 
Filter a collection using natural language
```csharp
var smallItems = items.Where(model, "item would fit in a bread box");
var bigItems = items.Remove(model, "item would fit in a bread box");
```

## enumerable.Select() 
.Select() let's you transform the source into target using an ITransformer model.

You can use it to transform an object from one format to another by simply giving the types. The model
will use AI to appropriately map the properties between the object types.
```csharp
var targetItems = items.Select<SourceItem,TargetItem>(model)
```

Transform a collection of text into another format, like markdown.
```csharp
var markdownItems = items.Select(model, "transform each item into markdown like this:\n# {{TITLE}}\n{{AUTHOR}}\n{{Description}}")
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

## enumerable.Answer() 
This operator let's you ask a question for each item in a collection.
```csharp
var answers = items.Answer<float>(model, "What is the cost?");
```

# ITransformer 
The ITransformer implements the core primatives for using AI to manipulate generation and transformation.

| Extension | Description | 
| ----------| ------------|
| ***.Generate()/.GenerateAsync()*** | use a model and a goal to return a shaped result. |
| ***.TransformItem()/.TransformItemAsync()*** | use a model and a goal to transform an item into  a shaped result. |
| ***.TransformItems()*** | use a model and a goal to transform a collection of items into a collection of shaped results. |

## transformer.Generate()
Given a model and a goal return a shaped result.
```csharp
var names = transformer.Generate<string[]>("funny names for people named bob");
var cities = transformer.Generate<City>("return the top 5 largest cities in the world.");
```

## transformer.TransformItem()
Given a model and a goal return a shaped result.
```csharp
var result = "my name is Tom".TransformItem<string>("translate to spanish); // ==> "Mi nombre es Tom"
```

## transformer.TransformItems()
Transform a collection of items using a model and a goal.
```csharp
var items = new string[] {"Hello", "My name is Tom", "One more please"];
var results = items.TransformItems("translate to spanish);
// result[0] = "Hola"
// result[1] = "Me llamo Tom"
// result[2] = "Una mas, por favor"
```

# Defining new operators
To create a custom operator you create an static class and define static methods for transforming an object or collection of objects.

For example, here is the implementation of Summarize():

* The ***SummarizeAsync()*** method defines object operator which calls **TransformItemAsync** with a default goal of "Create a summarization" with result type of string.
* The ***Summarize()*** method defines a collection operator which calls **TransformItems** with a default goal of "Create a summarization" with result type for each item in the collection of string.

```csharp
    public static class SummarizeExtension
    {
        // operator to summarize object
        public static string Summarize(this object source, ChatClient model, string? goal, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItem<string>(model, goal ?? "create a summarization", instructions, cancellationToken);

        // operator to summarize object
        public static Task<string> SummarizeAsync(this object source, ChatClient model, string? goal, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<string>(model, goal ?? "create a summarization", instructions, cancellationToken);

        // operator to summarize collection of objects
        public static IList<string> Summarize(this IEnumerable<object> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => source.TransformItems<string>(model, goal ?? "create a summarization", instructions, maxParallel, cancellationToken);
    }
```
