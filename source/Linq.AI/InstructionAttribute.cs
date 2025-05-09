using System;

namespace Linq.AI
{
    public class InstructionAttribute : Attribute
    {
        public InstructionAttribute(string instruction)
        {
            this.Instruction = instruction;
        }

        public string Instruction { get; set; }
    }
}
