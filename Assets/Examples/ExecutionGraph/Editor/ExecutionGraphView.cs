using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using UnityEditor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [CustomGraphView(typeof(ExecutionGraph))]
    public class ExecutionGraphView : BaseGraphView
    {
        public override Texture2D Icon => ResUtils.Load<Texture2D>("../Editor/icon-park-twotone--graphic-stitching-three.png");
        public override string Title => nameof(ExecutionGraph);
        public ExecutionGraphView(EditorWindow window) : base(window)
        {
        }
    }

}
