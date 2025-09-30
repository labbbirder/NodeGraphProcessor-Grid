using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/Game Object")]
public partial class GameObjectNode : BaseNode, ICreateNodeFrom<GameObject>
{
	[Output(name: "Out"), SerializeField]
	public GameObject output;

	public override string name => "Game Object";

	public bool InitializeNodeFromObject(GameObject value)
	{
		output = value;
		return true;
	}
}
