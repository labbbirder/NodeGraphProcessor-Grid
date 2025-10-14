using GraphProcessor;
using UnityEditor;
using UnityEditor.Callbacks;

namespace BBBirder.Graphs
{
	public class GraphAssetCallbacks
	{
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
}
