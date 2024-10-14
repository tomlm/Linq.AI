﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
