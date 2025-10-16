using BBBirder.Instructions;
using Cysharp.Threading.Tasks;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Math/Task")]
    public partial class EXTaskNode : LinearEXNode
    {
        [SerializeField, ShowInInspector] Procedure runInstructions;
        UniTask task;
        public override bool isRenamable => true;
        public override string name => "Task";

        public override void Enter()
        {
            task = runInstructions.RunAsync();
        }

        public override bool MoveNext()
        {
            return task.Status is UniTaskStatus.Pending;
        }
    }
}
