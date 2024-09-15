
using Iciclecreek.Async;
using Newtonsoft.Json;

namespace Linq.AI
{

    
    public static class CompareExtension
    {
        /// <summary>
        /// Compare 2 objects for semantic equivelancy
        /// </summary>
        /// <param name="transformer">transformer to use</param>
        /// <param name="item1">item1 to compare</param>
        /// <param name="item2">item2 to compare</param>
        /// <param name="instructions">instructions for how to compare</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>true/false</returns>
        public static bool Compare(this ITransformer transformer, object item1, object item2, string? instructions = null, CancellationToken cancellationToken = default)
            => transformer.CompareAsync(item1, item2, instructions, cancellationToken).Result;

        /// <summary>
        /// Compare 2 objects for semantic equivelancy async
        /// </summary>
        /// <param name="transformer">transformer to use</param>
        /// <param name="item1">item1 to compare</param>
        /// <param name="item2">item2 to compare</param>
        /// <param name="instructions">instructions for how to compare</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>true/false</returns>
        public static Task<bool> CompareAsync(this ITransformer transformer, object item1, object item2, string? instructions = null, CancellationToken cancellationToken = default)
        {
            item1 = item1 ?? String.Empty;
            item2 = item2 ?? String.Empty;
            string item1Text = ((item1 is string) ? item1 as string : JsonConvert.SerializeObject(item1))!;
            string item2Text = ((item2 is string) ? item2 as string : JsonConvert.SerializeObject(item2))!;
            return transformer.TransformItemAsync<bool>($"""
			<ITEM1>
			{item1Text}
			
			<ITEM2>
			{item2Text}
			""", 
            "are <ITEM1> and <ITEM2> semantically equivelent?", instructions, cancellationToken);
        }

    }
}

