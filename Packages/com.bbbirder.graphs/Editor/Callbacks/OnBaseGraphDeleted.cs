using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
	[ExecuteAlways]
	public class DeleteCallback : AssetModificationProcessor
	{
		static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
		{
			var objects = AssetDatabase.LoadAllAssetsAtPath(path);

			foreach (var obj in objects)
			{
				if (obj is IGraphOwner graphOwner && graphOwner.Graph is BaseGraph graph)
				{
					foreach (var graphWindow in Resources.FindObjectsOfTypeAll<BaseGraphWindow>())
						graphWindow.OnGraphDeleted();

					graph.OnAssetDeleted();
				}
			}

			return AssetDeleteResult.DidNotDelete;
		}
	}
}
