# Linq.AI.OpenAI
This library adds Linq extension methods using OpenAI structured outputs. 
> This library was heaviy inspired by stevenic's [agentm-js](https://github.com/stevenic/agentm-js) library, Kudos!

## Installation
```dotnet add package Linq.AI.OpenAI ```

## Architecture
For each element in a collection an model API call is made to evaluate and return the result. These are done in parallel on background threads.

## OpenAI model
To use these methods you will need to instantiate a ChatClient model like this:
```csharp
var model = new ChatClient(model: "gpt-4o-mini", "<modelKey>");
```
> NOTE: The model must support structured output.

# Object Extensions
These extensions use an OpenAI model to work with text.

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

Example using model to select 
```csharp
var words = text.Select<string>(model, "The second word of every paragraph");
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
These collection extensions use an OpenAI model to work with collections using Linq style methods.

| Extension | Description | 
| ----------| ------------|
| ***.Where()*** | Filter the collection of items by using a model. filter |
| ***.Select()*** | transform the item into another format using a model. |
| ***.Remove()*** | Remove items from a collection of items by using a model. filter |
| ***.Summarize()*** | Create a summarization for each item by using a model. |
| ***.Classify()*** | classify each item using a model. |
| ***.Answer()*** | get the answer to a question for each item using a model. |

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
var breadboxItems = items.Where(model, "item would fit in a bread box");
var bigItems = items.Remove(model, "item would fit in a bread box");
```

## enumerable.Select() 
.Select() let's you transform the source into target using an OpenAI model.

You can use it to transform an object from one format to another by simply giving the types. It will use model to do the transformation.
```csharp
var targetItems = items.Select<SourceItem,TargetItem>(model)
```

You can use it to transform a collection of text 
```csharp
var markdownItems = items.Select(model, "transform each item into markdown like this:\n# {{TITLE}}\n{{AUTHOR}}\n{{Description}}")
```

## enumerable.Summarize() 
Generate text summary for each item using an OpenAI model.

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

# Model Extensions
All of these extensions are built up using 3 ChatClient extensions as primitives.

| Extension | Description | 
| ----------| ------------|
| ***.Generate()/.GenerateAsync()*** | use a model and a goal to return a shaped result. |
| ***.TransformItem()/.TransformItemAsync()*** | use a model, object and goal to transform object into shaped result. |
| ***.TransformItems()/.TransformItemsAsync()*** | use a model, collection and goal to transform each object in the collection into a collection of shaped results. |

## model.Generate()/model.GenerateAsync()
Given a model and a goal return a shaped result.
```csharp
var names = model.Generate<string[]>("funny names for people named bob");
var cities = model.Generate<City>("return the top 5 largest cities in the world.");
```

## model.TransformItem()/model.TransformItemAsync()
Given a model and item, transform it into the shaped result using goal to guide the transformation.
```csharp
var piglatin = model.TransformItem<string>("cow", "transform to pig latin"); // outputs "owcay"
```

## model.TransformItem()/model.TransformItemAsync()
Given a model and item, transform it into the shaped result using goal to guide the transformation.
```csharp
var piglatin = model.TransformItems<string>(["cow", "dog", "pig"], "transform to pig latin"); // outputs ["owcay", "ogday", "igpay"]
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
