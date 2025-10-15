using System;

namespace BBBirder.Instructions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CategoryAttribute : Attribute
    {
        public readonly string Path;

        public CategoryAttribute(string path)
        {
            Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute
    {
        public readonly string Description;

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
