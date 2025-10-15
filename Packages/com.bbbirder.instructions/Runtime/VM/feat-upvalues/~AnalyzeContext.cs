using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace BBBirder.Instructions
{

    // public struct DebugTraceInfo
    // {
    //     public string Location;
    //     public int Offset;
    // }

    internal class AnalyzeContext
    {
        // public Stack<DebugTraceInfo> traceInfos = new();
        public Stack<StaticScope> traceStack = new();
        private StaticScope topFrame;
        public void Push(StaticScope stackFrame)
        {
            stackFrame.traceDepth = traceStack.Count;

            if (traceStack.Count > 0 && stackFrame.captureUpvalues)
            {
                stackFrame.InheritFrom(traceStack.Peek());
            }

            traceStack.Push(topFrame = stackFrame);
        }

        public bool Pop(out StaticScope result)
        {
            if (traceStack.TryPop(out result))
            {
                if (!traceStack.TryPeek(out topFrame))
                {
                    topFrame = null;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public T GetValue<T>(string name)
        {
            if (traceStack.TryPeek(out var topFrame))
            {
                return topFrame.GetValue<T>(name);
            }
            else
            {
                throw new("empty stack.");
            }
        }

        public void SetValue<T>(string name, T value)
        {
            throw new("Writing to parameters is inadvisable, as it can introduce potential issues and complications for the developers themselves.");
        }

        public void GetParametersInType<T>(List<string> result)
        {
            topFrame.GetParametersInType<T>(result);
        }

        // private StackFrame GetTopFrame()
        // {
        //     if (traceStack.TryPeek(out var topFrame))
        //     {
        //         return topFrame;
        //     }
        //     else
        //     {
        //         throw new("empty stack.");
        //     }
        // }
    }

}
