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
var model = new ChatClient(model: "gpt-4o-2024-08-06", "<modelKey>");
```
> NOTE: The model must support structured output.

# String Extensions
These extensions use an OpenAI model to work with text.

| Extension | Description | 
| ----------| ------------|
| ***.ClassifyAsync()*** | classify the text using a model. |
| ***.SummarizeAsync()*** | Create a summarization for the text by using a model. |
| ***.MatchesAsync()*** | Return whether the text matches using a model. |
| ***.AnswerAsync()*** | get the answer to a question from the text using a model. |
| ***.SelectAsync()*** | Generate a collection of items from the text using a model. |

## Examples

### .Classify() text

```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classification = await text.ClassifyAsync<Genres>(model);
```

### .Summarize() text

```csharp
var summary = await text.SummarizeAsync(model, "with 3 words");
```

### .Matches() text

```csharp
if (await text.MatchesAsync(model, "there is date"))
  ...
```

### .Answer() text

```csharp
var summary = await text.AnswerAsync(model, "what is the birthday?");
```

### .Select() text

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

# Collection Extensions 
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

## Examples

### .Classify() items
This allows you to classify each item using a model;
```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classifiedItems = items.Classify<Genres>(model);
```

### .Where()/.Remove() items
Filter a collection using natural language
```csharp
var breadboxItems = items.Where(model, "item would fit in a bread box");
var bigItems = items.Remove(model, "item would fit in a bread box");
```

### .Select() items
.Select() let's you transform the source into target using an OpenAI model.

#### Object transformation
You can use it to transform an object from one format to another by simply giving the types. It will use model. to do the transformation.
```csharp
var targetItems = items.Select<SourceItem,TargetItem>(model)
```

#### String transformation
```chsarp
var markdownItems = items.Select(model, "transform each item into markdown like this:\n# {{TITLE}}\n{{AUTHOR}}\n{{Description}}")
```

### .Summarize() items
Generate text summary for each item using an OpenAI model.

```chsarp
var summaries = items.Summarize(model);
```

You can control the summarization with a hint
```csharp
var summaries = items.Summarize(model, "generate a 3 word summary");
```

### .Answer() items
This operator let's you ask a question for each item in a collection.
```csharp
var answers = items.Answer(model, "What is total cost of the item as a float?").Select(answer => Convert.ToFloat(answer));
```

