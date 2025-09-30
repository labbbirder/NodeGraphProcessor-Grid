using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable]
	public class CopyPasteHelper
	{
		public List<JsonElement> copiedNodes = new();

		public List<JsonElement> copiedGroups = new();

		public List<JsonElement> copiedEdges = new();
	}
}
