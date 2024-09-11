# Linq.AI.OpenAI
This library adds Linq extension methods using OpenAI structured outputs.

## Architecture
For each element in a collection an openAI API call is made to evaluate and return the result. These are done in parallel on background threads.

## ChatClient
To use this you will need to instantiate a ChatClient like this:
```csharp
new ChatClient(model: "gpt-4o-2024-08-06", "<OpenAIKey>");
```

# Extensions

| Extension | Description | 
| ----------| ------------|
| ***.WhereAI()*** | Filter the collection of items by using a LLM filter |
| ***.SelectAI()*** | transform the item into another format using a LLM |
| ***.RemoveAI()*** | Remove items from a collection of items by using a LLM filter |
| ***.SummarizeAI()*** | Create a summarization for each item by using a LLM |
| ***.ClassifyAI()*** | classify each item using a LLM |
| ***.AnswerAI()*** | get the answer to a question for each item using a LLM |

# Examples

## .ClassifyAI( )
This allows you to classify each item using LLM.
```csharp
enum Genres { Rock, Pop, Electronica, Country, Classical };
var classifiedItems = items.ClassifyAI<Genres>(chatClient)).ToList();
```

## .WhereAI()/.RemoveAI()
This lets you filter a collection using natural language
```csharp
var breadboxItems = items.WhereAI(chatClient, "item would fit in a bread box")).ToList()
var bigItems = items.RemoveAI(chatClient, "item would fit in a bread box")).ToList()
```

## .SelectAI()
This lets you transform the source into another structure using natural language.

### Object transformation
You can use it to transform an object from one format to another by simply giving the types. It will use LLM to do the transformation.
```csharp
var targetItems = items.SelectAI<SourceItem,TargetItem>(chatClient"))
```

### String transformation
```chsarp
var markdownItems = items.SelectAI(chatClient, "transform each item into markdown like this:\n# {{TITLE}}\n{{AUTHOR}}\n{{Description}}")
```

## .SummarizeAI()
Generate text summary for each object.

```chsarp
var summaries= items.SummarizeAI(chatClient)
```

You can control the summarization with a hint
```csharp
var summaries = items.SummarizeAI(chatClient, "genreate a 3 paragraph summary");
```

## .AnswerAI()
This operator let's you ask a question for each item in a collection.
```csharp
var answers = items.AnswerAI(chatClient, "What is total cost of the item as a float?").Select(answer => Convert.ToFloat(answer));
```

# Chaining operations
Linq.AI operations and normal linq operations can be mixed together.
```csharp
var populations = items.SelectAI<SourceT, TargetT>(chatClient)
	.SummarizeAI(chatClient)
	.AnswerAI(chatClient, "What is the population?")
	.Select(answer => Convert.ToInt32(answer));
```
