using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/Prefab")]
public partial class PrefabNode : BaseNode
{
	[Output(name: "Out"), SerializeField]
	public GameObject output;

	public override string name => "Prefab";
}
