using System.Reflection;
using System.Runtime.CompilerServices;
using GraphProcessor;
using UnityEngine.Profiling;

namespace BBBirder.Graphs
{
    public class ExecutionBehaviour : GraphBehaviour<ExecutionGraph>
    {
        public void Update()
        {
            Graph.Run();
        }
    }
}
