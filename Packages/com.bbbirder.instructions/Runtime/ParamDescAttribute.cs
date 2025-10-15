using System;

namespace BBBirder.Instructions
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ParamDescAttribute : Attribute
    {
        public readonly int index;
        public readonly string name;
        public readonly string description;

        public ParamDescAttribute(int index, string name, string description = null)
        {
            this.index = index;
            this.name = name;
            this.description = description;
        }
    }
}
