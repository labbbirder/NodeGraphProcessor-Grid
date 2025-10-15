using System;
using System.Collections.Generic;
using BBBirder.Instructions;

namespace BBBirder.Instructions
{
    public class ParameterDescriptor
    {
        public int index;
        public string name;
        public string description;
        public Type type;
    }

    public class Scope
    {
        public Dictionary<string, ParameterDescriptor> paramDict = new();
        public List<ParameterDescriptor> paramList = new();
        public void PushArgument<T>(string name, string desc)
        {
            var param = new ParameterDescriptor()
            {
                index = paramList.Count,
                name = name,
                description = desc,
                type = typeof(T),
            };
            paramDict.Add(name, param);
            paramList.Add(param);
        }

        public void GetParametersOfType<T>(List<ParameterDescriptor> results)
        {
            foreach (var p in paramList)
            {
                if (typeof(T).IsAssignableFrom(p.type))
                {
                    results.Add(p);
                }
            }
        }

        public ParameterDescriptor GetArgumentAt(int index)
        {
            return paramList[index];
        }
    }

    internal static class Analyzer
    {
        // private static Stack<Scope> pool = new();
        private static Stack<Scope> trace = new();
        // internal static StaticScope RentFrame()
        // {
        //     Utils.CheckMainThread();
        //     return pool.Rent();
        // }

        // internal static void ReturnFrame(StaticScope stackFrame)
        // {
        //     Utils.CheckMainThread();
        //     pool.Return(stackFrame);
        // }
        public static void EnterScope(Scope scope)
        {
            trace.Push(scope);
        }

        public static void LeaveScope()
        {
            trace.Pop();
        }

        public static void PushArgument<T>(string name, string desc)
        {
            var top = trace.Peek();
            top.PushArgument<T>(name, desc);
        }

        public static void GetParametersOfType<T>(List<ParameterDescriptor> results)
        {
            var top = trace.Peek();
            top.GetParametersOfType<T>(results);
        }

        public static string GetArgumentName(int index)
        {
            var top = trace.Peek();
            return top.GetArgumentAt(index).name;
        }

    }
}

public struct ArgumentIndex
{
    // public int traceOffset;
    public int stackOffset;


    public T GetValue<T>()
    {
        // reduce cognitive load
        return ExecutionEngine.RuntimeShared.GetArgument<T>(-1, stackOffset);
    }
}
