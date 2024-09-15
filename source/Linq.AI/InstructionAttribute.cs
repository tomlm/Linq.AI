using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
