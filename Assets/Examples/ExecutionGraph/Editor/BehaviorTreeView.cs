using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEditor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [CustomGraphView(typeof(BehaviorTree))]
    public class BehaviorTreeView : BaseGraphView
    {
        public override Texture2D Icon => ResUtils.Load<Texture2D>("../Editor/material-symbols--graph-2.png");
        public override string Title => nameof(BehaviorTree);
        public BehaviorTreeView(EditorWindow window) : base(window)
        {
        }
    }

}
