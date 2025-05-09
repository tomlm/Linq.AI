namespace Linq.AI
{
    public class Utils
    {
        public static string GetItemIndexClause(int index, string? instructions)
        {
            return $"""
                    The collection starts with Index: 0
                    This Item's Index is: {index} 
                    {instructions ?? string.Empty}
                    """;
            //return $"""
            //        First Index: 0
            //        Last Index: {count - 1}
            //        This Item Index: {index} 
            //        {instructions ?? string.Empty}
            //        """;
        }
    }
}
