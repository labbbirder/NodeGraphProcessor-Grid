using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Math/Add")]
    public partial class MultiAddNode : EXNode
    {
        [Input(unpack: true)]
        public float[] inputs;

        [Output, ShowAsDrawer]
        public float output;

        public override string name => "Add";

        protected override void AfterPullDatas()
        {
            output = 0;
            foreach (var n in inputs)
            {
                output += n;
            }
        }
    }
}
