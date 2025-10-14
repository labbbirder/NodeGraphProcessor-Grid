using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Logic/Comparison")]
    public partial class Comparison : EXNode
    {
        [Input(name: "In A")]
        [ShowAsDrawer]
        public float inA;

        [Input(name: "In B")]
        [ShowAsDrawer]
        public float inB;

        [Output(name: "Out")]
        [ShowAsDrawer]
        public bool compared;

        public CompareFunction compareFunction = CompareFunction.LessEqual;

        // protected override bool IsDataFlowDeterministic => true;
        public override string name => "Comparison";

        protected override void AfterPullDatas()
        {
            switch (compareFunction)
            {
                default:
                case CompareFunction.Disabled:
                case CompareFunction.Never: compared = false; break;
                case CompareFunction.Always: compared = true; break;
                case CompareFunction.Equal: compared = inA == inB; break;
                case CompareFunction.Greater: compared = inA > inB; break;
                case CompareFunction.GreaterEqual: compared = inA >= inB; break;
                case CompareFunction.Less: compared = inA < inB; break;
                case CompareFunction.LessEqual: compared = inA <= inB; break;
                case CompareFunction.NotEqual: compared = inA != inB; break;
            }
        }
    }
}
