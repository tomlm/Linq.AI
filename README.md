# Linq.AI.OpenAI
This library adds Linq extension methods using OpenAI structured outputs. (heaviy inspired by stevenic's [agentm-js](https://github.com/stevenic/agentm-js) library)

## Architecture
For each element in a collection an openAI API call is made to evaluate and return the result. These are done in parallel on background threads.

## OpenAI ChatClient
To use this you will need to instantiate a openAI like this:
```csharp
var openai = new openAI(model: "gpt-4o-2024-08-06", "<OpenAIKey>");
```

# Extensions
These extensions all take a openAI() as parameter and use OpenAI API to resolve.

| Extension | Description | 
| ----------| ------------|
| ***.Where()*** | Filter the collection of items by using a LLM filter |
| ***.Select()*** | transform the item into another format using a LLM |
| ***.Remove()*** | Remove items from a collection of items by using a LLM filter |
| ***.Summarize()*** | Create a summarization for each item by using a LLM |
| ***.Classify()*** | classify each item using a LLM |
| ***.Answer()*** | get the answer to a question for each item using a LLM |

# Examples

## .Classify( )
This allows you to classify each item using LLM.
```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classifiedItems = items.Classify<Genres>(openAI).ToList();
```

## .Where()/.Remove()
This lets you filter a collection using natural language
```csharp
var breadboxItems = items.Where(openAI, "item would fit in a bread box").ToList()
var bigItems = items.Remove(openAI, "item would fit in a bread box").ToList()
```

## .Select()
This lets you transform the source into another structure using natural language.

### Object transformation
You can use it to transform an object from one format to another by simply giving the types. It will use LLM to do the transformation.
```csharp
var targetItems = items.Select<SourceItem,TargetItem>(openAI")
```

### String transformation
```chsarp
var markdownItems = items.Select(openAI, "transform each item into markdown like this:\n# {{TITLE}}\n{{AUTHOR}}\n{{Description}}")
```

## .Summarize()
Generate text summary for each object.

```chsarp
var summaries= items.Summarize(openAI)
```

You can control the summarization with a hint
```csharp
var summaries = items.Summarize(openAI, "genreate a 3 paragraph summary");
```

## .Answer()
This operator let's you ask a question for each item in a collection.
```csharp
var answers = items.Answer(openAI, "What is total cost of the item as a float?").Select(answer => Convert.ToFloat(answer));
```

# Chaining operations
Linq.AI operations and normal linq operations can be mixed together.
```csharp
var populations = items.Select<SourceT, TargetT>(openAI)
	.Summarize(openAI)
	.Answer(openAI, "What is the population?")
	.Select(answer => Convert.ToInt32(answer));
```
