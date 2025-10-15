using System;
using System.Collections.Generic;

namespace BBBirder.Instructions
{
    internal class StackFrame
    {
        public int traceDepth;
        private readonly List<RuntimeArgument> arguments = new();

        public void Push(in RuntimeArgument arg)
        {
            arguments.Add(arg);
        }

        public RuntimeArgument GetArgumentAt(int stack)
        {
            if (stack >= 0 || -stack > arguments.Count)
            {
                throw new IndexOutOfRangeException($"stack {stack} out of range [-1, -{arguments.Count}]");
            }

            return arguments[~stack];
        }

        public void Clear()
        {
            traceDepth = -1;
            foreach (var arg in arguments)
            {
                if (arg.parameterType.IsValueType)
                {
                    (arg.reference as IColumnBuffer)?.RemoveByToken(arg.subkey);
                }
            }

            arguments.Clear();
        }
    }
}
