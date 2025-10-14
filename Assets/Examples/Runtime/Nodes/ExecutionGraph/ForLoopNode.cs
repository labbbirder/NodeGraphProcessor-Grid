using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Control/ForLoop")]
    public partial class ForLoopNode : EXNode
    {
        [Input(name: "Executed")]
        public ExecutionLink executed;

        [Output(name: "Loop Body")]
        public ExecutionLink loopBody;

        [Output(name: "Completed")]
        public ExecutionLink loopCompleted;


        [Output]
        public int index;

        [Input, ShowAsDrawer]
        public int start = 0;

        [Input, ShowAsDrawer]
        public int end = 10;

        public override string name => "ForLoop";

        public override void Enter()
        {
            index = start - 1;
        }

        public override bool MoveNext()
        {
            if (index++ >= end)
            {
                EnqueueExecutionPort(nameof(loopCompleted));
                return false;
            }
            else
            {
                EnqueueExecutionPort(nameof(loopBody));
                return true;
            }
        }
    }
}
