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
| ***.ClassifyAsync()*** | classify the text using a model. |
| ***.SummarizeAsync()*** | Create a summarization for the text by using a model. |
| ***.MatchesAsync()*** | Return whether the text matches using a model. |
| ***.AnswerAsync()*** | get the answer to a question from the text using a model. |
| ***.SelectAsync()*** | Generate a collection of items from the text using a model. |

## Object .Classify() 

```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classification = await item.ClassifyAsync<Genres>(model);
```

## item.Summarize() 

```csharp
var summary = await item.SummarizeAsync(model, "with 3 words");
```

## item.Matches() 

```csharp
if (await item.MatchesAsync(model, "there is date"))
  ...
```

## item.Answer() 

```csharp
var summary = await text.AnswerAsync(model, "what is the birthday?");
```

## item.Select() 
Select pulls a collection of items from the source.

Example using model to select 
```csharp
var words = await text.SelectAsync<string>(model, "The second word of every paragraph");
```

Example using model to select structed data.
```csharp
public class HREF 
{ 
	public string Url {get;set;}
	public string Title {get;set;}
}
var summary = await text.SelectAsync<HREF>(model);
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

> NOTE: These methods are synchronous linq methods, but internally they run all of the AI calls as throttled parallel background tasks.

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

# Defining new operators
All of these operators are built up of 2 core operators
* ***TransformItemAsync()*** - which allows you to give a transformation goal and instructions for a single item.
* ***TransformItems()*** - Which allows you a transformation goal and instructions for each element in a enumerable collection.

To create a custom operator you create an static class and define static methods for transforming an object or collection of objects.

For example, here is the implementation of Summarize():

* The ***SummarizeAsync()*** method defines object operator which calls **TransformItemAsync** with a default goal of "Create a summarization" with result type of string.
* The ***Summarize()*** method defines a collection operator which calls **TransformItems** with a default goal of "Create a summarization" with result type for each item in the collection of string.

```csharp
    public static class SummarizeExtension
    {
        // Object operator
        public static Task<string> SummarizeAsync(this object source, ChatClient model, string? goal, string? instructions = null, CancellationToken cancellationToken = default)
            => source.TransformItemAsync<string>(model, goal ?? "create a summarization", instructions, cancellationToken);

        // collection operator
        public static IList<string> Summarize(this IEnumerable<object> source, ChatClient model, string? goal = null, string? instructions = null, int? maxParallel = null, CancellationToken cancellationToken = default)
            => source.TransformItems<string>(model, goal ?? "create a summarization", instructions, maxParallel, cancellationToken);
    }
```