using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [Serializable, NodeMenuItem("Debug/Console Log")]
    public partial class ConsoleNode : LinearEXNode
    {
        public override string name => "Console Log";

        [Input("Object")]
        public object obj;

        [Setting("Log Type")]
        public LogType logType = LogType.Log;

        public override void Enter()
        {
            Debug.Log(obj);
        }

        // public override IEnumerable<ConditionalNode> CoroutineProcess()
        // {
        //     switch (logType)
        //     {
        //         case LogType.Error:
        //         case LogType.Exception:
        //             Debug.LogError(obj != null ? obj.ToString() : logText);
        //             throw new("ddd");
        //             break;
        //         case LogType.Assert:
        //             Debug.LogAssertion(obj != null ? obj.ToString() : logText);
        //             break;
        //         case LogType.Warning:
        //             Debug.LogWarning(obj != null ? obj.ToString() : logText);
        //             break;
        //         case LogType.Log:
        //             Debug.Log(obj != null ? obj.ToString() : logText);
        //             break;
        //     }
        //     return base.CoroutineProcess();
        // }

        // protected override void Process()
        // {
        // 	switch(logType)
        // 	{
        // 		case LogType.Error:
        // 		case LogType.Exception:
        // 			Debug.LogError(obj != null ? obj.ToString() : logText);
        // 			break;
        // 		case LogType.Assert:
        // 			Debug.LogAssertion(obj != null ? obj.ToString() : logText);
        // 			break;
        // 		case LogType.Warning:
        // 			Debug.LogWarning(obj != null ? obj.ToString() : logText);
        // 			break;
        // 		case LogType.Log:
        // 			Debug.Log(obj != null ? obj.ToString() : logText);
        // 			break;
        // 	}
        // }
    }
}
