using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

public enum Setting
{
	S1,
	S2,
	S3,
}

[System.Serializable, NodeMenuItem("Custom/SettingsNode")]
public partial class SettingsNode : BaseNode
{
	public Setting setting;
	public override string name => "SettingsNode";

	[Input]
	public float input;

	[Output]
	public float output;

}
