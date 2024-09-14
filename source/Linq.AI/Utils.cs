namespace Linq.AI
{
    public class Utils
    {
        public static string GetItemIndexClause(int index, int count, string? instructions)
        {
            return $"""
                    First Index: 0
                    Last Index: {count - 1}
                    This Item Index: {index} 
                    {instructions ?? string.Empty}
                    """;
        }
    }
}
