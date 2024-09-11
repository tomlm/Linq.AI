# Linq.AI.OpenAI
This library adds Linq extension methods using OpenAI structured outputs. (heaviy inspired by stevenic's [agentm-js](https://github.com/stevenic/agentm-js) library)

## Architecture
For each element in a collection an openAI API call is made to evaluate and return the result. These are done in parallel on background threads.

## ChatClient
To use this you will need to instantiate a ChatClient like this:
```csharp
new ChatClient(model: "gpt-4o-2024-08-06", "<OpenAIKey>");
```

# Extensions
These extensions all take a ChatClient() as parameter and use OpenAI API to resolve.

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
var classifiedItems = items.ClassifyAI<Genres>(chatClient)).ToList();
```

## .Where()/.Remove()
This lets you filter a collection using natural language
```csharp
var breadboxItems = items.Where(chatClient, "item would fit in a bread box")).ToList()
var bigItems = items.Remove(chatClient, "item would fit in a bread box")).ToList()
```

## .Select()
This lets you transform the source into another structure using natural language.

### Object transformation
You can use it to transform an object from one format to another by simply giving the types. It will use LLM to do the transformation.
```csharp
var targetItems = items.SelectAI<SourceItem,TargetItem>(chatClient"))
```

### String transformation
```chsarp
var markdownItems = items.Select(chatClient, "transform each item into markdown like this:\n# {{TITLE}}\n{{AUTHOR}}\n{{Description}}")
```

## .Summarize()
Generate text summary for each object.

```chsarp
var summaries= items.Summarize(chatClient)
```

You can control the summarization with a hint
```csharp
var summaries = items.Summarize(chatClient, "genreate a 3 paragraph summary");
```

## .Answer()
This operator let's you ask a question for each item in a collection.
```csharp
var answers = items.Answer(chatClient, "What is total cost of the item as a float?").Select(answer => Convert.ToFloat(answer));
```

# Chaining operations
Linq.AI operations and normal linq operations can be mixed together.
```csharp
var populations = items.SelectAI<SourceT, TargetT>(chatClient)
	.Summarize(chatClient)
	.Answer(chatClient, "What is the population?")
	.Select(answer => Convert.ToInt32(answer));
```
