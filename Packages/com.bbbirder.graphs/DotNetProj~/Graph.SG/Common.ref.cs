using System;

namespace GraphProcessor
{
    /// <summary>
    /// Tell that this field is will generate an input port
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InputAttribute : Attribute
    {
        public readonly string Name;
        public readonly bool Hide;
        public readonly bool Unpack;

        /// <summary>
        /// Mark the field as an input port
        /// </summary>
        /// <param name="name">display name</param>
        /// <param name="unpack">unpack array to elements</param>
        public InputAttribute(string name = null, bool unpack = false, bool hide = false)
        {
            this.Name = name;
            this.Hide = hide;
            this.Unpack = unpack;
        }
    }

    public class OutputAttribute:Attribute { }

    public class BaseNode { }

    public struct ExecutionLink { }
}
