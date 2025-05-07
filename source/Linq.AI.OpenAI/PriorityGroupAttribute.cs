namespace Linq.AI.OpenAI
{
    /// <summary>
    /// PriorityGroup on a method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PriorityGroupAttribute : Attribute
    {
        public PriorityGroupAttribute(int group)
        {
            Group = group;
        }

        public int Group { get; }
    }
}
