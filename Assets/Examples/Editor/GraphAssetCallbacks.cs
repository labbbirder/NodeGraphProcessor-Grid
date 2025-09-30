using System.Collections;
using System.Collections.Generic;
using System.IO;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class GraphAssetCallbacks
{
	// [MenuItem("Assets/Create/GraphProcessor", false, 10)]
	// public static void CreateGraphPorcessor()
	// {
	// 	var graph = ScriptableObject.CreateInstance<BaseGraph>();
	// 	ProjectWindowUtil.CreateAsset(graph, "GraphProcessor.asset");
	// }

	[OnOpenAsset(0)]
	public static bool OnBaseGraphOpened(int instanceID, int line)
	{
		var graphOwner = EditorUtility.InstanceIDToObject(instanceID) as IGraphOwner;
		if (graphOwner != null)
		{
			EditorWindow.GetWindow<BaseGraphWindow>().InitializeGraph(graphOwner as UnityEngine.Object);
			return true;
		}
		return false;
	}
}
